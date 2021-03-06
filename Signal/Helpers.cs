using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

namespace Signal
{
    public class Helpers
    {
        private bool isInitialized;
        public bool IsInitialized
        {
            get => isInitialized;
            set
            {
                isInitialized = value;
                NotifyStateChanged();
            }
        }
        public event Action OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();
        private HttpClient _Http;

        public string DataPath;
        public string ImgPath;

        private string textCommon;
        public string TextCommon
        {
            get
            {
                if (string.IsNullOrEmpty(TextCommonStore) && dataSettings != null) return dataSettings.TextCommon;
                else return TextCommonStore;
            }
            set
            {
                textCommon = value;
                NotifyStateChanged();
            }
        }

        public string TextCommonStore;
        public bool SoundOn;
        public bool ImgOn;
        public bool TextCommonOn;
        public bool TextPointOn;
        public int TimePreSound;
        public double Volume;


        public DataSettings dataSettings;
        public SequenceCities[] sequenceCities;

        Pages.Index _Index;
        IConfiguration _Configuration;

        public async Task Initialize(HttpClient http, Blazored.LocalStorage.ILocalStorageService localStorage, IConfiguration configuration, Pages.Index index = null)
        {
            _Index = index;
            _Http = http;
            _Configuration = configuration;

            DataPath = await localStorage.GetItemAsync<string>("dataPath");
            if (string.IsNullOrEmpty(DataPath))
                DataPath = configuration["dataPath"];

            var s1 = await localStorage.GetItemAsync<bool>("soundOn");
            var s2 = configuration.GetValue<bool>("SoundOn");
            SoundOn = s1 | s2;

            ImgOn = await localStorage.GetItemAsync<bool>("imgOn");
            TextCommonOn = await localStorage.GetItemAsync<bool>("textCommonOn");
            TextPointOn = await localStorage.GetItemAsync<bool>("textPointOn");
            TextCommonStore = await localStorage.GetItemAsync<string>("textCommon");
            TimePreSound = await localStorage.GetItemAsync<int>("timePreSound");
            Volume = await localStorage.GetItemAsync<double>("volume");

            await LoadData();

            isInitialized = true;
        }

        public async Task LoadData()
        {
            if (!string.IsNullOrEmpty(DataPath))
            {
                Random rnd = new Random();
                int rndValue = rnd.Next();

                try
                {
                    dataSettings = await _Http.GetFromJsonAsync<DataSettings>(DataPath + "?v=" + rndValue);
                    //sequenceCities = await _Http.GetFromJsonAsync<SequenceCities[]>(DataPath + "?v=" + rndValue);
                }
                catch (Exception ex)
                {
                    dataSettings = null;
                }
            }
            else dataSettings = null;

            Array.ForEach(dataSettings.SequencePoints, x => {
                x.Date = DateTime.Now.Date.Add(new TimeSpan(x.Date.Value.Hour, x.Date.Value.Minute, x.Date.Value.Second));
            });

            sequenceCities = dataSettings.SequencePoints;
            if (string.IsNullOrEmpty(TextCommonStore)) TextCommon = dataSettings.TextCommon;
            else TextCommon = TextCommonStore;
        }

        /// <summary>
        /// Получаем остаток секунд до ближайшего Посыла (обратный отсчЁт)
        /// </summary>
        /// <returns></returns>
        public CountdownView Countdown()
        {
            if (sequenceCities == null) return null;

            var now = DateTime.Now;// .AddDays(5).AddHours(17)

            var pointNext = sequenceCities.Aggregate(
                (x, y) =>
                {
                    //x.Date - now > y.Date - now ? x : y
                    var t = (x.Date - now).Value.TotalSeconds;
                    return t < 0 ? y : x;
                }
            ); // выбираем ближайшую большую сигнальную точку

            var index = Array.IndexOf(sequenceCities, pointNext);
            var pointCur = index > 0 ? sequenceCities[index - 1] : pointNext;
            var tmleft = (long)(pointNext.Date - now)?.TotalSeconds;
            if (tmleft < 0)
            {
                pointCur = pointNext;
                pointNext = null;
                tmleft = 0;
            }

            return new CountdownView { PointCur = pointCur, PointNext = pointNext, Seconds = tmleft };
        }

        public string RefreshTimer()
        {
            var t = "null";
            if (_Index.timer != null)
            {
                t = _Index.timer.ToString();
                _Index.timer.Dispose();
            }
            _Index.CreateTimer();

            var timerMessage = _Configuration.GetValue<bool>("TimerMessage");

            return timerMessage ? t : null;
        }

    }

    public class DataSettings
    {
        public string TextCommon { get; set; }
        public SequenceCities[] SequencePoints { get; set; }
    }

    public class SequenceCities
    {
        public DateTime? Date { get; set; }

        public string Point { get; set; }
        public string Text { get; set; }
        public bool Common { get; set; }
    }

    public class CountdownView
    {
        public SequenceCities PointCur { get; set; }
        public SequenceCities PointNext { get; set; }
        public long Seconds { get; set; }
    }
}
