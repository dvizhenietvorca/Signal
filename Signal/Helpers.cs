using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

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

        public string DataPath;// = "sample-data/cities.json";


        public SequenceCities[] sequenceCities;

        public async Task Initialize(HttpClient http, Blazored.LocalStorage.ILocalStorageService localStorage)
        {
            _Http = http;

            if (string.IsNullOrEmpty(DataPath))
                DataPath = await localStorage.GetItemAsync<string>("dataPath");

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
                    sequenceCities = await _Http.GetFromJsonAsync<SequenceCities[]>(DataPath + "?v=" + rndValue);
                }
                catch (Exception)
                {
                }
            }
            else sequenceCities = null;
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
                    //var t1 = (x.Date - now).Value.Seconds;
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

            //return tmleft < 0 ? -1 : tmleft;
            return new CountdownView { PointCur = pointCur, PointNext = pointNext, Seconds = tmleft };
        }

    }

    public class SequenceCities
    {
        public DateTime? Date { get; set; }

        public string City { get; set; }
    }

    public class CountdownView
    {
        public SequenceCities PointCur { get; set; }
        public SequenceCities PointNext { get; set; }
        public long Seconds { get; set; }
    }
}
