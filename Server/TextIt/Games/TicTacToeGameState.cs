using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using TextIt.Hubs;
using TextIt.Models;

namespace TextIt.Games
{

    public class TicTacToeGameState : GameState
    {
        public GameBoard Board { get; set; }
        public class GameBoard
        {
            public BoardItem this[int index]
            {
                get { return Items[index]; }
                set { Items[index] = value; }
            }

            public BoardItem[] Items { get; set; }

            public class BoardItem
            {
                public const string TopLeft = "top left";
                public const string TopRight = "top right";
                public const string TopMiddle = "top";
                public const string MiddleLeft = "left";
                public const string MiddleMiddle = "";
                public const string MiddleRight = "right";
                public const string BottomLeft = "bottom left";
                public const string BottomMiddle = "bottom";
                public const string BottomRight = "bottom right";

                public string Position { get; set; }
                public BoardValue Value { get; set; }
                public enum BoardValue
                {
                    None = 0,
                    Cross = 1,
                    Naught = 2
                }

                public BoardItem(string position, BoardValue value)
                {
                    Position = position;
                    Value = value;
                }

                public BoardItem()
                {
                    
                }
            }
        }
        public string LastTurnUserId { get; set; }
        public Dictionary<string, GameBoard.BoardItem.BoardValue> BoardValues { get; set; }
        public override void LoadFromSave(string save)
        {
            var me = JsonConvert.DeserializeObject<TicTacToeGameState>(save);
            Board = me.Board;
            BoardValues = me.BoardValues;
            LastTurnUserId = me.LastTurnUserId;
        }

        public override string Save()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override void SetupStart()
        {
            if(Game?.Owner == null)
                throw new InvalidOperationException();
            Board = new GameBoard
            {
                Items = new []
                {
                    new GameBoard.BoardItem(GameBoard.BoardItem.TopLeft, GameBoard.BoardItem.BoardValue.None),
                    new GameBoard.BoardItem(GameBoard.BoardItem.TopMiddle, GameBoard.BoardItem.BoardValue.None),
                    new GameBoard.BoardItem(GameBoard.BoardItem.TopRight, GameBoard.BoardItem.BoardValue.None),

                    new GameBoard.BoardItem(GameBoard.BoardItem.MiddleLeft, GameBoard.BoardItem.BoardValue.None),
                    new GameBoard.BoardItem(GameBoard.BoardItem.MiddleMiddle, GameBoard.BoardItem.BoardValue.None),
                    new GameBoard.BoardItem(GameBoard.BoardItem.MiddleRight, GameBoard.BoardItem.BoardValue.None),

                    new GameBoard.BoardItem(GameBoard.BoardItem.BottomLeft, GameBoard.BoardItem.BoardValue.None),
                    new GameBoard.BoardItem(GameBoard.BoardItem.BottomMiddle, GameBoard.BoardItem.BoardValue.None),
                    new GameBoard.BoardItem(GameBoard.BoardItem.BottomRight, GameBoard.BoardItem.BoardValue.None),
                }
            };
            BoardValues = new Dictionary<string, GameBoard.BoardItem.BoardValue>
            {
                [Game.Owner.Id] = GameBoard.BoardItem.BoardValue.Cross,
            };
        }

        public override void AddPlayer(ApplicationUser user)
        {
            if(BoardValues.Count == 2) return;

            BoardValues[user.Id] = (GameBoard.BoardItem.BoardValue) (BoardValues.Count + 1);
        }

        public override void UpdateState(dynamic newState, HubCallerContext context)
        {
            var currentUserId = LobbyHub.UserMatches[context.ConnectionId];

            if (newState["turn"] != null)
            {
                if (LastTurnUserId == currentUserId) return;
                if (!BoardValues.ContainsKey(currentUserId)) return;
                var index = (int) newState["turn"]["index"];
                if (0 <= index && index < Board.Items.Length)
                {
                    var boardItem = Board[index];
                    if (boardItem.Value == GameBoard.BoardItem.BoardValue.None)
                    {
                        LastTurnUserId = currentUserId;
                        boardItem.Value = BoardValues[currentUserId];
                        newState.turn.value = boardItem.Value;
                        UpdateGameState(new OnGameStateUpdateArgs(newState, context));
                    }
                }
            }

            CheckFinshed();
        }

        

        private void CheckFinshed()
        {
            Func<string, GameBoard.BoardItem> getItemFromPos = pos => Board.Items.First(x => x.Position == pos);
            for (int i = 1; i < 3; i++)
            {

                var value = (GameBoard.BoardItem.BoardValue)i;
                //#
                // #
                //  #
                if (getItemFromPos(GameBoard.BoardItem.TopLeft).Value == value &&
                    getItemFromPos(GameBoard.BoardItem.MiddleMiddle).Value == value &&
                    getItemFromPos(GameBoard.BoardItem.BottomRight).Value == value)
                {
                    EndGame(new OnGameEndArgs { Dispose = true, Reason = "Player " + BoardValues.First(x => x.Value == value).Key + " Won" });
                    return;
                }
                //  #
                // #
                //#
                if (getItemFromPos(GameBoard.BoardItem.TopRight).Value == value &&
                    getItemFromPos(GameBoard.BoardItem.MiddleMiddle).Value == value &&
                    getItemFromPos(GameBoard.BoardItem.BottomLeft).Value == value)
                {
                    EndGame(new OnGameEndArgs { Dispose = true, Reason = "Player " + BoardValues.First(x => x.Value == value).Key + " Won" });
                    return;
                }
                //###
                //
                //
                if (getItemFromPos(GameBoard.BoardItem.TopLeft).Value == value &&
                    getItemFromPos(GameBoard.BoardItem.TopMiddle).Value == value &&
                    getItemFromPos(GameBoard.BoardItem.TopRight).Value == value)
                {
                    EndGame(new OnGameEndArgs { Dispose = true, Reason = "Player " + BoardValues.First(x => x.Value == value).Key + " Won" });
                    return;
                }
                //
                //###
                //
                if (getItemFromPos(GameBoard.BoardItem.MiddleLeft).Value == value &&
                    getItemFromPos(GameBoard.BoardItem.MiddleMiddle).Value == value &&
                    getItemFromPos(GameBoard.BoardItem.MiddleRight).Value == value)
                {
                    EndGame(new OnGameEndArgs { Dispose = true, Reason = "Player " + BoardValues.First(x => x.Value == value).Key + " Won" });
                    return;
                }
                //
                //
                //###
                if (getItemFromPos(GameBoard.BoardItem.BottomLeft).Value == value &&
                    getItemFromPos(GameBoard.BoardItem.BottomMiddle).Value == value &&
                    getItemFromPos(GameBoard.BoardItem.BottomRight).Value == value)
                {
                    EndGame(new OnGameEndArgs { Dispose = true, Reason = "Player " + BoardValues.First(x => x.Value == value).Key + " Won" });
                    return;
                }
                //#
                //#
                //#
                if (getItemFromPos(GameBoard.BoardItem.TopLeft).Value == value &&
                    getItemFromPos(GameBoard.BoardItem.MiddleLeft).Value == value &&
                    getItemFromPos(GameBoard.BoardItem.BottomLeft).Value == value)
                {
                    EndGame(new OnGameEndArgs { Dispose = true, Reason = "Player " + BoardValues.First(x => x.Value == value).Key + " Won" });
                    return;
                }
                // #
                // #
                // #
                if (getItemFromPos(GameBoard.BoardItem.TopMiddle).Value == value &&
                    getItemFromPos(GameBoard.BoardItem.MiddleMiddle).Value == value &&
                    getItemFromPos(GameBoard.BoardItem.BottomMiddle).Value == value)
                {
                    EndGame(new OnGameEndArgs { Dispose = true, Reason = "Player " + BoardValues.First(x => x.Value == value).Key + " Won" });
                    return;
                }
                //  #
                //  #
                //  #
                if (getItemFromPos(GameBoard.BoardItem.TopLeft).Value == value &&
                    getItemFromPos(GameBoard.BoardItem.MiddleLeft).Value == value &&
                    getItemFromPos(GameBoard.BoardItem.BottomLeft).Value == value)
                {
                    EndGame(new OnGameEndArgs { Dispose = true, Reason = "Player " + BoardValues.First(x => x.Value == value).Key + " Won" });
                    return;
                }

            }
        }

        public TicTacToeGameState(Game game) : base(game)
        {
        }
    }
}