using System.Drawing;
using System.IO;
using AForge;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using ZXing.Aztec;
using ZXing.Common;
using ZXing.QrCode;
using BarcodeReader = ZXing.Windows.Compatibility.BarcodeReader;

namespace SIDBookConsole
{
    internal class Program
    {
        static FilterInfoCollection CaptureDevice { get; set; }
        static VideoCaptureDevice FinalFrame { get; set; }
        static Bitmap LatestFrame { get; set; }
        static DateTime? WaitTime { get; set; } = null;

        static void Main(string[] args)
        {
            Console.Title = "모의 방명록 시스템 v1.0";

            while (true)
            {
                if (!InitializeWebcam()) continue;


                string? input = Console.ReadLine();

                if (input == null) break;
                
            }

            if (FinalFrame.IsRunning == true)
            {
                FinalFrame.Stop();
            }
        }

        static bool InitializeWebcam()
        {
            CaptureDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            int i = 1;

            foreach (FilterInfo Device in CaptureDevice)
            {
                Console.WriteLine($"{i}. {Device.Name}");
                i++;
            }
            Console.Write("\n사용할 웹캠의 번호를 입력해주세요 : ");

            string? input = Console.ReadLine();

            if (int.TryParse(input, out int number))
            {
                FinalFrame = new VideoCaptureDevice(CaptureDevice[number - 1].MonikerString);
                FinalFrame.NewFrame += new NewFrameEventHandler(FinalFrame_NewFrame);
                FinalFrame.Start();
            }
            else
            {
                Console.WriteLine("다시 입력해주세요.");
                return false;
            }

            Console.WriteLine("\n");
            const string format = "{0,-15}{1,-15}{2,-30}{3,-30}";
            Console.WriteLine(format, "학번", "이름", "QR 코드를 찍은 날짜", "QR 코드를 생성한 날짜");
            Console.WriteLine(new string('=', 100));

            return true;
        }

        static void FinalFrame_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            LatestFrame = (Bitmap)eventArgs.Frame.Clone();
            string decoded = ParseImage();

            if (!string.IsNullOrEmpty(decoded))
            {
                if (WaitTime.HasValue && DateTime.Now < WaitTime) return;
                
                WaitTime = DateTime.Now.AddSeconds(3);
                string[] data = decoded.Split(";");
                string id = data[0];
                string name = data[1];
                string generated = data[2];

                const string format = "{0,-15}{1,-15}{2,-40}{3,-40}";
                Console.WriteLine(format, id, name, GetDateFormat(DateTime.Now), GetDateFormat(ParseGeneratedDate(generated)));
            }
        }

        static string ParseImage()
        {
            BarcodeReader Reader = new BarcodeReader
            {
                AutoRotate = true,
                Options = new DecodingOptions { TryHarder = true, CharacterSet = "utf-8" }
            };
            Result result = Reader.Decode(LatestFrame);

            string decoded = result.ToString().Trim();
            return decoded;
        }

        static string GetDateFormat(DateTime date)
        {
            return $"{date.Year}/{date.Month}/{date.Day} {date.Hour}:{date.Minute}:{date.Second}";
        }

        static DateTime ParseGeneratedDate(string data)
        {
            string year1 = data.Substring(0, 4);
            int year2 = int.Parse(year1);
            string month1 = data.Substring(4, 2);
            int month2 = int.Parse(month1);
            string day1 = data.Substring(6, 2);
            int day2 = int.Parse(day1);
            string hour1 = data.Substring(8, 2);
            int hour2 = int.Parse(hour1);
            string minute1 = data.Substring(10, 2);
            int minute2 = int.Parse(minute1);
            string second1 = data.Substring(12, 2);
            int second2 = int.Parse(second1);

            return new DateTime(year2, month2, day2, hour2, minute2, second2);
        }
    }
}
