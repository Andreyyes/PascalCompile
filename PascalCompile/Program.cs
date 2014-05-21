﻿using System;
using System.IO;

namespace PascalCompile
{
    class Program
    {
        static void Main(string[] args)
        {           
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
                        //Console.WriteLine();
                        //e.Dump();
                        //Console.WriteLine();
                        Console.WriteLine(code.Remove(0, cursor.start).Remove(cursor.end - cursor.start));
                    } while (cursor != null);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
                
                //Console.WriteLine(pc.GetCode());
            }
            

            Console.ReadKey(true);
        }
    }
}
