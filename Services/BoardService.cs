﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using dotnetserver.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;


namespace dotnetserver
{
    public interface IBoardService
    {
        Task ChangeBoardInformation(Board newBoard);
        Task<IEnumerable<Board>> GetBoards(string userId);
        Task AddNewBoard(Board newBoard, string userId);
        Task DeleteBoard(string boardId);

    }
    public class BoardService: WithDbAccess, IBoardService
    {
        private readonly ILogger<BoardService> _logger;
        public BoardService(ILogger<BoardService> logger, ConnectionContext context) : base(context)
        {
            _logger = logger;
        }

        public async Task ChangeBoardInformation(Board newBoard)
        {
            var sql = @"UPDATE board SET 
                        boardName=@boardName,
                        boardDescription=@boardDescription
                        WHERE boardId=@boardId";
            await DbExecuteAsync(sql, newBoard);
        }
        public async Task<IEnumerable<Board>>GetBoards(string userId)
        {
            var parameters = new { UserId = userId };
            var sql = "SELECT * FROM board WHERE userId=@UserId";
            return await DbQueryAsync<Board>(sql, parameters);
        }

        public async Task AddNewBoard(Board newBoard, string userId)
        {
            _logger.LogDebug($"User[{userId}] has started creation new board with: " +
                             $"newBoard.boardDescription - {newBoard.boardDescription}, " +
                             $"newBoard.boardName - {newBoard.boardName}");
            newBoard.userId = uint.Parse(userId);
            var sql = @"INSERT INTO board(
                        userId, boardName,
                        boardDescription) 
                        VALUES(
                        @userId, @boardName,
                        @boardDescription);
                        SELECT boardId FROM board WHERE userId=@userId";
            var boardId = await DbQueryAsync<uint>(sql, newBoard);
            newBoard.boardId = boardId.Last();
        }

        public async Task DeleteBoard(string boardId)
        {
            var parameters = new { BoardId = boardId };
            var sql = @"CALL DeleteUserBoard(@BoardId)";
            await DbExecuteAsync(sql, parameters);
        }

    }
}