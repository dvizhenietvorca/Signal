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


        public SequenceCities[] sequenceCities;

        public async Task Initialize(HttpClient Http)
        {
            sequenceCities = await Http.GetFromJsonAsync<SequenceCities[]>("sample-data/cities.json");
            isInitialized = true;
        }

        /// <summary>
        /// Получаем остаток секунд до ближайшего Посыла (обратный отсчЁт)
        /// </summary>
        /// <returns></returns>
        public CountdownView Countdown()
        {
            var now = DateTime.Now;// .AddDays(5).AddHours(17)

            var near = sequenceCities.Aggregate(
                (x, y) =>
                {
                    //x.Date - now > y.Date - now ? x : y
                    return (x.Date - now).Value.Seconds < 0 ? y : x;
                }
            ); // выбираем ближайшую большую сигнальную точку

            var tmleft = (long)(near.Date - now)?.TotalSeconds;

            //return tmleft < 0 ? -1 : tmleft;
            return new CountdownView { Data = near, Seconds = tmleft };
        }

    }

    public class SequenceCities
    {
        public DateTime? Date { get; set; }

        public string City { get; set; }
    }

    public class CountdownView
    {
        public SequenceCities Data { get; set; }
        public long Seconds { get; set; }
    }
}
