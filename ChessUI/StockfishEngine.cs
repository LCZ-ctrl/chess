using ChessLogic;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ChessUI
{
    public class StockfishEngine : IDisposable
    {
        private readonly Process _process;
        private readonly StreamWriter _writer;
        private readonly StreamReader _reader;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public StockfishEngine()
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "Engines/stockfish.exe",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            _process.Start();
            _writer = _process.StandardInput;
            _reader = _process.StandardOutput;
            Initialize();
        }

        private void Initialize()
        {
            SendSync("uci");
            Expect("uciok");
            SendSync("isready");
            Expect("readyok");
        }

        private void SendSync(string cmd)
        {
            _writer.WriteLine(cmd);
            _writer.Flush();
        }

        private string ReadLineSync()
        {
            return _reader.ReadLine();
        }

        private void Expect(string expected)
        {
            string? line;
            while ((line = ReadLineSync()) != null)
            {
                if (line.Contains(expected))
                    return;
            }
            throw new InvalidOperationException($"Stockfish did not respond with '{expected}'");
        }

        public async Task<string> GetBestMoveUciAsync(string movesUci, int skillLevel, CancellationToken ct)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                SendSync("ucinewgame");

                if (!string.IsNullOrEmpty(movesUci))
                {
                    SendSync($"position startpos moves {movesUci}");
                }
                else
                {
                    SendSync("position startpos");
                }

                SendSync($"setoption name Skill Level value {skillLevel}");

                SendSync("go depth 12");

                while (!ct.IsCancellationRequested)
                {
                    string? line = await _reader.ReadLineAsync();
                    if (line == null)
                        throw new InvalidOperationException("Stockfish EOF");

                    if (line.StartsWith("bestmove "))
                    {
                        var parts = line.Split(' ');
                        return parts[1];
                    }
                }

                ct.ThrowIfCancellationRequested();
                return null!;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            try
            {
                SendSync("quit");
            }
            catch { }
            try
            {
                _process.Kill();
                _process.WaitForExit(2000);
            }
            catch { }
            _process.Dispose();
            _semaphore.Dispose();
        }
    }
}