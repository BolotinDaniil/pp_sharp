using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace phills2017
{
    class Phills2017
    {
        static bool debug_mode = false;

        static int phill_count = 5;
        static short THINK = 0;
        static short EAT = 1;

        static List<Thread> threads = new List<Thread>();
        static List<short> phill_states = new List<short>();
        static List<int> phill_requests = new List<int>();
        static List<bool> free_forks = new List<bool>();

        static int slave_ttl = 500; //100
        static bool slave_alive = true;

        //static Queue<int> queue_ = new Queue<int>(); // очередь обращений
        static object superlock = new object();

        static void Main(string[] args)
        {
            for (int i = 0; i < phill_count; i++) // Список состояний философов
            {
                phill_states.Add(THINK);
                phill_requests.Add(0);
                free_forks.Add(true);

                threads.Add(new Thread(new ParameterizedThreadStart(tPhill)));
                threads[i].Name = "Phill_" + i;
            }

            Thread Slave = new Thread(new ParameterizedThreadStart(tSlave));
            Slave.Name = "Slave";

            Slave.Start();
            for (int i = 0; i < phill_count; i++)
                threads[i].Start(i);
            
            // ...

            // Join all threads
            for (int i = 0; i < phill_count; i++)
                threads[i].Join();
            Slave.Join();

            if (debug_mode)
            {
                Console.WriteLine("Main end.");
                Console.ReadLine();
            }

        }//Main

        static void tPhill(object index)
        {
            int number = Convert.ToInt32(index);
            int left = number, 
                right = (number + 1) % phill_count;
            
            double tmp = 0;
            Random rnd = new Random();

            while (slave_alive) {
                if (phill_states[number] == THINK) {

                    // Thread.Sleep(5);
                    for (int i = 0; i < 10000000; i++) // think process
                        tmp = (1 / tmp + i * 1.1);

                    lock (phill_requests)
                        phill_requests[number]++; // ??? ask

                    tmp = 0;
                    Thread.Sleep(10);
                }

                if (phill_states[number] == EAT) {

                    lock (phill_requests)
                    { // ??? to slaave
                        //Console.WriteLine("BITCH"); // CONSOLE WRITE MTHRFKR 
                        phill_states[number] = THINK;
                        free_forks[left] = true;
                        free_forks[right] = true;
                        phill_requests[number] = 0;
                    }
                    Thread.Sleep(10);
                }
            }

            if (debug_mode)
                Console.WriteLine("phil {0} end.", index);
        }//Phill

        static void tSlave(object index)
        {
            int tmp = 0,
                max_index = 0;
            int left, right;

            for (int i = 0; i < slave_ttl; i++) {

                lock (phill_requests)
                {
                    max_index = phill_requests.IndexOf(phill_requests.Max());
                    // --
                    left = max_index;
                    right = (max_index + 1) % phill_count;

                    if (free_forks[left] && free_forks[right])
                    { // ?? lock
                        //Console.WriteLine("BITCH__{0}", max_index);
                        free_forks[left] = false;
                        free_forks[right] = false;
                        phill_requests[max_index] = 0;
                        phill_states[max_index] = EAT;
                    }
                    //--
                }
                Thread.Sleep(5);

                if (debug_mode) {
                    /*Console.WriteLine("___" + max_index);
                    lock (phill_requests)
                    {
                        phill_requests.ForEach(Console.Write);
                        Console.WriteLine();
                    } */
                }
            }
                
            if (debug_mode)
                Console.WriteLine("Slave end");

            slave_alive = false;
        }//Slave

    }
}
