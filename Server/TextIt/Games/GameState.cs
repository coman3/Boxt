using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using TextIt.Models;
#pragma warning disable 1591

namespace TextIt.Games
{
    public abstract class GameState
    {
        public event OnGameStateUpdate OnGameStateUpdate;
        public event OnGameEnd OnGameEnd;
        [JsonIgnore]
        public bool Running { get; set; }
        public Game Game { get; set; }
        public Stack<string> PlayerTurns { get; set; }
        public Dictionary<string, int> PlayerWins { get; set; }
        protected GameState(Game game)
        {
            Game = game;
            PlayerTurns = new Stack<string>();
            PlayerWins = new Dictionary<string, int>();
        }
        public abstract void LoadFromSave(string save);
        public void LoadFromCompressedSave(string data)
        {
            var bytes = Convert.FromBase64String(data);
            var saveData = Unzip(bytes);
            LoadFromSave(saveData);
        }
        public void LoadFromCompressedSave()
        {
            LoadFromCompressedSave(Game.GameState);
        }

        public abstract string Save();

        public string SaveCompressed()
        {
            var data = Save();
            var zip = Zip(data);
            return Convert.ToBase64String(zip);
        }
        public abstract void SetupStart();
        public abstract void AddPlayer(ApplicationUser user);

        protected virtual void EndGame(OnGameEndArgs args)
        {
            OnGameEnd?.Invoke(this, args);
        }

        protected void UpdateGameState(OnGameStateUpdateArgs args)
        {
            OnGameStateUpdate?.Invoke(this, args);
        }
        public abstract void UpdateState(dynamic newState, HubCallerContext context);
        

        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }
        private static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }
        private static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

    }

    public delegate void OnGameStateUpdate(GameState sender, OnGameStateUpdateArgs args);

    public class OnGameStateUpdateArgs : EventArgs
    {
        public dynamic State { get; set; }
        public HubCallerContext Context { get; set; }

        public OnGameStateUpdateArgs(dynamic state, HubCallerContext context)
        {
            State = state;
            Context = context;
        } 

    }

    public delegate void OnGameEnd(GameState sender, OnGameEndArgs args);

    public class OnGameEndArgs : EventArgs
    {
        public string WinnerUserId { get; set; }
        public GameWinReason Reason { get; set; }
        public dynamic ExtraData { get; set; }
        public bool Dispose { get; set; }

        public OnGameEndArgs(GameWinReason reason, dynamic extraData = null, bool dispose = false)
        {
            WinnerUserId = null;
            Reason = reason;
            Dispose = dispose;
            ExtraData = extraData;
        }
        public OnGameEndArgs(GameWinReason reason, string winnerUserId, dynamic extraData = null, bool dispose = false)
        {
            WinnerUserId = winnerUserId;
            Reason = reason;
            Dispose = dispose;
            ExtraData = extraData;
        }
        public enum GameWinReason
        {
            PlayerWin,
            Timeout,
        }
    }
}