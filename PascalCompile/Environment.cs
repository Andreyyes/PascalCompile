using System;
using System.Collections.Generic;

public class Environs
{
    /// <summary>
    /// Структура переменной, хранит переменную и её реальное имя
    /// </summary>
    public struct EnvironsStuct
    {
        /// <summary>
        /// Хранимая переменная
        /// </summary>
        public Variable value;

        /// <summary>
        /// Имя хранимой переменной
        /// </summary>
        public string name;
    };

    /// <summary>
    /// Список окружения, хранятся переменные типа EnvironsStruct
    /// </summary>
    private List<EnvironsStuct> enviroment;
    private Calculation calc;

    public Environs()
    {
        enviroment = new List<EnvironsStuct>();
        calc = new Calculation(this);
    }

    /// <summary>
    /// Добавляет переменную в окружение
    /// </summary>
    /// <param name="var">Переменная</param>
    /// <param name="name">Имя переменной</param>
    public void Add(Variable var, string name)
    {
        EnvironsStuct es = new EnvironsStuct();
        es.name = name;
        es.value = var;
        enviroment.Add(es);
    }

    /// <summary>
    /// Возвращает переменную по её имени
    /// </summary>
    /// <param name="name">Имя переменной</param>
    /// <returns>Переменная</returns>
    public Variable GetElementByName(string name)
    {
        string[] replace = new string[] { ".", "^", "[", "]" };
        for (int i = 0; i < replace.Length; i++)
            name = name.Replace(replace[i], "☺" + replace[i] + "☺");
        string[] words = name.Split(new char[] { '☺' }, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
            throw new Exception("Нельзя обратиться к пустому имени переменной");

        Variable var = GetElementByBaseName(words[0]);

        int opn_bkt = 0;
        bool get_field = false;
        string mas_index = "";

        for (int i = 1; i < words.Length; i++)
        {
            if (words[i] == "^")
            {
                if (var.GetType().Name == "Pointer")
                    var = ((Pointer)var).Value;
                else
                    throw new Exception("Нельзя применять операцию разыменования к статической переменной");
            }
            else if (words[i] == "[" && opn_bkt == 0)
            {
                if (var.GetType().Name != "Massiv")
                    throw new Exception("Нельзя обратиться к переменной типа " + var.GetType().Name + " как к массиву");
                opn_bkt = 1;
            }
            else if (words[i] == "[")
            {
                opn_bkt++;
            }
            else if (words[i] == "]" && opn_bkt == 1)
            {
                opn_bkt = 0;
                object index = null;
                if (!TryCalculate(mas_index, out index))
                    throw new Exception("Нельзя использовать выражение " + mas_index + " в качестве индекс массива");
                if (var.GetType().Name != "Massiv")
                    throw new Exception("Для обращения к индексу массива переменная должна быть типа Array");
                var = ((Massiv)var).GetVariable(index.ToString());
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
            }
            else if (get_field)
            {
                if (var.GetType().Name != "Record")
                    throw new Exception("Для обращения к полю переменная должна быть типа Record");
                var = ((Record)var).GetVariable(words[i]);
                get_field = false;
            }
        }

        return var;

        /*string[] names = name.Split('.');

        if (names.Length == 0)
            return new NullVariable();

        Variable var = new NullVariable();
        for (int i = 0; i < names.Length; i++)
        {
            bool pointer = names[i].EndsWith("^");
            if (pointer) names[i] = names[i].Remove(names[i].Length - 1);

            if (i == 0)
            {
                foreach (EnvironsStuct item in enviroment)
                {
                    if (item.name == names[i])
                        var = item.value;
                }
            }
            else
            {
                if (var.GetType().Name == "NullVariable")
                    break;
                else if (var.GetType().Name == "Record")
                    var = ((Record)var).GetVariable(names[i]);
            }

            if (pointer)
                var = ((Pointer)var).Value;
        }

        return var;*/
    }

    /// <summary>
    /// Получает базовую переменную из окружения
    /// </summary>
    /// <param name="name">Корневое имя переменной</param>
    /// <returns></returns>
    public Variable GetElementByBaseName(string name)
    {
        foreach (EnvironsStuct item in enviroment)
        {
            if (item.name == name)
                return item.value;
        }
        return new NullVariable();
    }

    public void DeleteElementByName(string name)
    {

    }

    /// <summary>
    /// Получает список статических переменных (не являются ссылками и их нельзя удалить)
    /// </summary>
    /// <returns></returns>
    public List<EnvironsStuct> GetStaticVariable()
    {
        List<EnvironsStuct> lst = new List<EnvironsStuct>();
        foreach (EnvironsStuct es in enviroment)
            if (!es.value.pointer && es.name != "DYNAMIC")
                lst.Add(es);
        return lst;
    }

    /// <summary>
    /// Получает список указателей, указатель может быть создан динамически
    /// </summary>
    /// <returns></returns>
    public List<EnvironsStuct> GetPointerVariable()
    {
        List<EnvironsStuct> lst = new List<EnvironsStuct>();
        foreach (EnvironsStuct es in enviroment)
            if (es.value.pointer)
                lst.Add(es);
        return lst;
    }

    /// <summary>
    /// Получает список переменных, которые привели к утечке памяти, 
    /// они не связаны с имененем переменной или указателем (могут быть связаны между собой)
    /// </summary>
    /// <returns></returns>
    public List<EnvironsStuct> GetTrushVariable()
    {
        List<EnvironsStuct> lst = new List<EnvironsStuct>();
        foreach (EnvironsStuct es in enviroment)
            if (es.name == "DYNAMIC")
                lst.Add(es);
        foreach (EnvironsStuct es in GetPointerVariable())
            for (int i = 0; i < lst.Count; i++)
                if (lst[i].value == es.value.value)
                    lst.RemoveAt(i--);
        return lst;
    }

    /// <summary>
    /// Присваивает переменной значение операнда
    /// </summary>
    /// <param name="var_name">Переменная</param>
    /// <param name="operand_name">Операнд</param>
    public void AssignmentVar(string var_name, string operand_name)
    {
        Variable var = GetElementByName(var_name);
        object result;
        if (TryCalculate(operand_name, out result) && result != null)
        {
            switch (var.GetType().Name)
            {
                case "Boolean":
                    if (result.GetType().Name != "Boolean")
                        throw new Exception("Нельзя присвоить переменной типа boolean значени типа " + result.GetType().Name);
                    ((Boolean)var).Value = (bool)result;
                    return;
                case "Real":
                    if (result.GetType().Name != "Double")
                        throw new Exception("Нельзя присвоить переменной типа real значение типа " + result.GetType().Name);
                    ((Real)var).Value = (double)result;
                    return;
                case "Integer":
                    if (result.GetType().Name != "Double")
                        throw new Exception("Нельзя присвоить переменной типа integer значение типа " + result.GetType().Name);
                    if ((int)(double)result != (double)result)
                        throw new Exception("Нельзя присвоить переменной типа integer значение типа real");
                    ((Integer)var).Value = (int)(double)result;
                    return;
                default:
                    throw new Exception("Нельзя присвоить переменной типа " + var.GetType().Name +
                        " значение типа " + result.GetType().Name);
            }
        }
        
        Variable operand = GetElementByName(operand_name);

        if (var.GetType().Name != operand.GetType().Name)
            throw new Exception("Нельзя присвоить переменной с типом " + var.GetType().Name 
                + " значение переменной с типом " + operand.GetType().Name);
        if (var.pointer)
            if (operand.pointer)
                AssignmentLink(var_name, operand);
            else
                throw new Exception("Нельзя присвоить значение переменной указателю");
        else
            if (var.GetType().Name == "Record")
                ((Record)var).SetValue((Record)operand);
            else
                var.value = operand.value;
    }

    /// <summary>
    /// Присваивает переменной значение выражения
    /// </summary>
    /// <param name="var_name">Имя переменной</param>
    /// <param name="expression">Выражение</param>
    public void AssignmentExpr(string var_name, string expression)
    {
        object result = Calculate(expression);
        Variable var = GetElementByName(var_name);

        if (result.GetType().Name == "Boolean" && var.GetType().Name == "Boolean")
            var.value = result;
        else if (result.GetType().Name == "Double")
        {
            if (var.GetType().Name == "Integer")
            {
                if ((int)(double)result == (double)result)
                    var.value = (int)(double)result;
                else
                    throw new Exception("Нельзя присвоить целочисленной переменной вещественное значение");
            }
            else if (var.GetType().Name == "Real")
                var.value = result;
            else
                throw new Exception("Нельзя присвоить переменной типа " + var.type + " значение типа " + result.GetType().Name);
        }
        else
            throw new Exception("Нельзя присвоить переменной типа " + var.type + " значение типа " + result.GetType().Name);
    }

    /// <summary>
    /// Присвоить переменной с имененим pointer_name адрес переменной variable_name
    /// </summary>
    /// <param name="pointer_name">Имя ссылки на переменну.</param>
    /// <param name="variable_name">Имя переменной, адрес которой нужно присвоить</param>
    public void AssignmentAddr(string pointer_name, string variable_name)
    {
        Variable pointer = GetElementByName(pointer_name);
        if (pointer == null)
            throw new Exception("Используется необъявленная переменная: " + pointer_name);
        Variable variable = GetElementByName(variable_name);
        if (variable == null)
            throw new Exception("Используется необъявленная переменная: " + variable_name);
        if (!pointer.pointer)
            throw new Exception("Операция new не применима к статическим типам");
        if (pointer.type != "pointer" && pointer.type != variable.type)
            throw new Exception("Тип указателя и переменной не совпадают");
        ((Pointer)pointer).Value = variable;
    }

    /// <summary>
    /// Связывает переменную указатель с новым адресом, на которую она должна ссылаться
    /// </summary>
    /// <param name="var_name">Имя указателя</param>
    /// <param name="operand">Новы адрес</param>
    public void AssignmentLink(string var_name, Variable operand)
    {
        string[] names = var_name.Split('.');

        if (names.Length == 0)
            return;
        else if (names.Length == 1)
        {
            if (names[0].EndsWith("^"))
                throw new Exception("Нельзя к разыменованной ссылке присвоить значение адреса");
            else
                for (int i = 0; i < enviroment.Count; i++)
                    if (enviroment[i].name == names[0])
                    {
                        enviroment.RemoveAt(i);
                        Add(operand, var_name);
                    }
        }
        else
        {
            Variable var = new NullVariable();
            for (int i = 0; i < names.Length - 1; i++)
            {
                bool pointer = names[i].EndsWith("^");
                if (pointer) names[i] = names[i].Remove(names[i].Length - 1);

                if (i == 0)
                {
                    foreach (EnvironsStuct item in enviroment)
                        if (item.name == names[i])
                            var = item.value;
                }
                else
                {
                    if (var.GetType().Name == "NullVariable")
                        break;
                    else if (var.GetType().Name == "Record")
                        var = ((Record)var).GetVariable(names[i]);
                }

                if (pointer)
                    var = ((Pointer)var).Value;
            }

            if (names[names.Length - 1].EndsWith("^"))
                throw new Exception("Нельзя к разыменованной ссылке присвоить значение адреса");
            else
                if (var.GetType().Name == "Record")
                    ((Record)var).SetVariable(operand, names[names.Length - 1]);
        }
    }

    /// <summary>
    /// Производит математические вычисления над переданной строкой
    /// </summary>
    /// <param name="expr">Выражение</param>
    /// <returns>double или boolean</returns>
    public object Calculate(string expr)
    {
        return calc.Calculate(expr);
    }

    /// <summary>
    /// Пытается посчитать выражение и записывает результат в переменную output. Возвращает true при успешном подсчёте и false в случае ошибки
    /// </summary>
    /// <param name="expr">Выражение</param>
    /// <param name="output">Переменная для вывода результата</param>
    /// <returns>boolean</returns>
    public bool TryCalculate(string expr, out object output)
    {
        output = null;
        try
        {
            output = Calculate(expr);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dump()
    {
        foreach (EnvironsStuct item in enviroment)
            if (item.value.GetType().Name == "Record")
                ((Record)item.value).Dump();
            else if (item.value.GetType().Name == "Massiv")
                ((Massiv)item.value).Dump();
            else
            Console.WriteLine("{0}({1}) = {2};",
                item.name,
                item.value.GetType().Name,
                item.value.value);
    }
}
