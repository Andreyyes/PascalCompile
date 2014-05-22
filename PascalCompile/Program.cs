using System;
using System.Collections.Generic;
using System.IO;

namespace PascalCompile
{
    class Program
    {
        static void Main(string[] args)
        {
            /*try
            {
                Environs env = new Environs();
                object result;
                result = env.Calculate("'Hello ' + 'wld!'");
                bool b = env.TryCalculate("'Hello ' + 'world!'", out result);
                Console.WriteLine(b);
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.ReadKey(true);*/


            /*string name = "rec^.x^[5.3+0.7]^.y";
            Console.WriteLine(name);

            string[] replace = new string[] { ".", "^", "[", "]" };
            for (int i = 0; i < replace.Length; i++)
                name = name.Replace(replace[i], "☺" + replace[i] + "☺");
            string[] words = name.Split(new char[] { '☺' }, StringSplitOptions.RemoveEmptyEntries);

            Console.WriteLine("BASE NAME {0}", words[0]);

            int opn_bkt = 0;
            bool get_field = false;
            string mas_index = "";
            for (int i = 1; i < words.Length; i++)
            {
                if (words[i] == "^")
                {
                    Console.WriteLine("Получить ссылку");
                }
                else if (words[i] == "[" && opn_bkt == 0)
                {
                    opn_bkt = 1;
                    Console.WriteLine("Начало индекса массива");
                }
                else if (words[i] == "[")
                {
                    opn_bkt++;
                }
                else if (words[i] == "]" && opn_bkt == 1)
                {
                    opn_bkt = 0;
                    Console.WriteLine("Конец индекса массива");
                    Console.WriteLine("Индекс массива == {0}", mas_index);
                    mas_index = "";
                }
                else if (words[i] == "]")
                {
                    opn_bkt--;
                }
                else if (opn_bkt > 0)
                {
                    mas_index += words[i];
                }
                else if (words[i] == ".")
                {
                    get_field = true;
                } else if (get_field) {
                    Console.WriteLine("Обратиться к полю {0}", words[i]);
                    get_field = false;
                }

                //Console.WriteLine(words[i]);
                Console.ReadKey(true);
            }
            Console.ReadKey(true);*/




            string file = "pas.txt";
            if (File.Exists(file))
            {
                Console.WriteLine(file);

                StreamReader sr = new StreamReader(file);

                string code = sr.ReadToEnd();
                sr.Close();

                try
                {
                    ParseCode pc = new ParseCode(code);
                    Environs e = pc.GetEnviroment();
                    //e.Dump();

                    Tree cursor = null;
                    do
                    {
                        Console.ReadKey(true);
                        cursor = pc.DoNextCommand();
                        if (cursor == null)
                        {
                            Console.WriteLine("Программа завершилась.");
                            continue;
                        }

                        List<Environs.EnvironsStuct> lst = e.GetTrashVariable();

                        foreach (Environs.EnvironsStuct item in lst)
                        {
                            Console.WriteLine("{0}({1}) = {2};",
                                item.name,
                                item.value.GetType().Name,
                                item.value.value);
                        }

                        //Console.WriteLine();
                        //e.Dump();
                        //Console.WriteLine();
                        //Console.WriteLine(code.Remove(0, cursor.start).Remove(cursor.end - cursor.start));
                    } while (cursor != null);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
                
                //Console.WriteLine(pc.GetCode());
            }

            while (true) ;
            Console.ReadKey(true);
        }
    }
}
