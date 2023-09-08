using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace DesktopRec
{
    public class Recorder
    {
        public MMDevice SelectDevice()
        {
            MMDeviceCollection playbackDevices =
                new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            int selectedDeviceIndex = 0;

            while (true)
            {
                Console.Clear();

                Console.WriteLine("Available output devices:");

                for (int i = 0; i < playbackDevices.Count; i++)
                {
                    string deviceName = playbackDevices[i].FriendlyName;
                    if (i == selectedDeviceIndex)
                    {
                        deviceName = $"[{deviceName}]";
                    }

                    Console.WriteLine($"{i + 1}. {deviceName}");
                }

                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (keyInfo.Key == ConsoleKey.UpArrow && selectedDeviceIndex > 0)
                {
                    selectedDeviceIndex--;
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow && selectedDeviceIndex < playbackDevices.Count - 1)
                {
                    selectedDeviceIndex++;
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    break;
                }
            }

            MMDevice selectedOutputDevice = playbackDevices[selectedDeviceIndex];

            return selectedOutputDevice;
        }

        public void Record()
        {
            var selectedOutputDevice = this.SelectDevice();

            WasapiOut playback = new WasapiOut(selectedOutputDevice, AudioClientShareMode.Shared, true, 100);
            WasapiLoopbackCapture capture = new();
            capture.WaveFormat = new WaveFormat(44100, 16, 2);

            WaveFileWriter writer = new("output.wav", capture.WaveFormat);

            long totalBytesRecorded = 0;
            const long bytesPerMegabyte = 1024 * 1024;

            capture.DataAvailable += (s, e) =>
            {
                writer.Write(e.Buffer, 0, e.BytesRecorded);
                totalBytesRecorded += e.BytesRecorded;

                double megabytesRecorded = (double)totalBytesRecorded / bytesPerMegabyte;

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"Recording... {megabytesRecorded:F2} MB recorded. Press any key to stop.");
            };

            capture.RecordingStopped += (s, e) =>
            {
                writer.Dispose();
                capture.Dispose();
            };

            capture.StartRecording();

            Console.WriteLine("Recording started. Press any key to stop recording. ");

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    capture.StopRecording();
                    playback.Dispose();

                    Console.WriteLine("Recording stopped. Audio file saved as output.wav.");
                    return;
                }

                Thread.Sleep(100);
            }
        }
    }
}