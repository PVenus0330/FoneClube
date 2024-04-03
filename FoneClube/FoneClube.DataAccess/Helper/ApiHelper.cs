using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.DataAccess.Utilities
{
    public static class ApiHelper
    {
        public static int PoolSize { get => apiClientPool.Size; }

        private static ArrayPool<HttpClient> apiClientPool = new ArrayPool<HttpClient>(() => {
            var apiClient = new HttpClient();
            apiClient.DefaultRequestHeaders.Add("ContentType", "application/json");
            return apiClient;
        });

        public static Task Use(string apiToken, Func<HttpClient, Task> action)
        {
            return apiClientPool.Use(client => {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiToken);
                return action(client);
            });
        }
    }

    public class ArrayPool<T>
    {
        public int Size { get => pool.Count(); }
        public int maxSize = 3;
        public int circulingObjectCount = 0;
        private Queue<T> pool = new Queue<T>();
        private Func<T> constructorFunc;

        public ArrayPool(Func<T> constructorFunc)
        {
            this.constructorFunc = constructorFunc;
        }

        public Task Use(Func<T, Task> action)
        {
            T item = GetNextItem(); //DeQueue the item
            var t = action(item);
            t.ContinueWith(task => pool.Enqueue(item)); //Requeue the item
            return t;
        }

        private T GetNextItem()
        {
            //Create new object if pool is empty and not reached maxSize
            if (pool.Count == 0 && circulingObjectCount < maxSize)
            {
                T item = constructorFunc();
                circulingObjectCount++;
                Console.WriteLine("Pool empty, adding new item");
                return item;
            }
            //Wait for Queue to have at least 1 item
            WaitForReturns();

            return pool.Dequeue();
        }

        private void WaitForReturns()
        {
            long timeouts = 60000;
            while (pool.Count == 0 && timeouts > 0) { timeouts--; System.Threading.Thread.Sleep(1); }
            if (timeouts == 0)
            {
                throw new Exception("Wait timed-out");
            }
        }
    }
}
