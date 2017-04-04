using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
// using System.Text;
// using System.Threading.Tasks;


namespace phills2017
{
    class Phills2017
    {
        static bool debug_mode = true;

        const int phill_count = 5;
        const short THINK = 0;
        const short EAT = 1;

        static List<Thread> threads = new List<Thread>();
        static List<short> phill_states = new List<short>();  // состояния философов
        static List<int> phill_requests = new List<int>();  // количество просьб о еде от каждого философа - лист на phill_count элементов
        static List<bool> free_forks = new List<bool>();  // статус вилок

        const int slave_ttl = 250; //100  
        static bool slave_alive = true;
        const int slave_sleep = 5; // ms
        const int phill_sleep = 50; // ms

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

            // Join all threads
            for (int i = 0; i < phill_count; i++)
                threads[i].Join();
            Slave.Join();

            if (debug_mode)
            {
                Console.WriteLine("Main thread end.");
                Console.ReadLine();
            }

        }//Main

        static void intense_work()
        {
            double tmp = 0;
            for (int i = 0; i < 100000000; i++)
                tmp = (1 / tmp + i * 1.1);
        }

        static void tPhill(object index)
        {
            int number = Convert.ToInt32(index);
            int left = number, 
                right = (number + 1) % phill_count;

            while (slave_alive) {
                if (phill_states[number] == THINK) {

                    Thread.Sleep(phill_sleep);  // ++

                    lock (superlock)  // DO REQUEST.
                        phill_requests[number]++;
                }

                if (phill_states[number] == EAT) {
                    lock (superlock) {
                        if (debug_mode)
                            Console.WriteLine("phill {0} EAT end.", number); // CONSOLE WRITE MTHRFKR 
                        phill_states[number] = THINK;
                        free_forks[left] = true;
                        free_forks[right] = true;
                        phill_requests[number] = 0;
                        
                    }
                    intense_work(); // THINK PROCESS.  // ++


                }
            }

            if (debug_mode)
                Console.WriteLine("phil {0} end.", index);
        }//Phill

        static void tSlave(object index)
        {
            int max_index = 0;
            int left, right;

            for (int i = 0; i < slave_ttl; i++) {

                lock (superlock)
                {
                    max_index = phill_requests.IndexOf(phill_requests.Max()); // Выбираем самого голодного.

                    left = max_index;
                    right = (max_index + 1) % phill_count;

                    if (free_forks[left] && free_forks[right])
                    {
                        if (debug_mode)
                            Console.WriteLine("set phill {0} to EAT", max_index);

                        free_forks[left] = false;
                        free_forks[right] = false;
                        phill_requests[max_index] = 0;
                        phill_states[max_index] = EAT;
                    }
                }
                Thread.Sleep(slave_sleep);
            }
                
            if (debug_mode)
                Console.WriteLine("Slave end");

            slave_alive = false;
        }//Slave

    }//class
}
