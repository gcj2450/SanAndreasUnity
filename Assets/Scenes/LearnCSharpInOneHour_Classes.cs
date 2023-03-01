using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static LearnCSharpInOneHour;

public class Chef
{
    public void MakeChiken()
    {
        Console.WriteLine("The Chef makes chicken");
    }

    public void MakeSalad()
    {
        Console.WriteLine("The Chef makes salad");
    }

    //Virtual means this method can be override in any sub-class
    public virtual void MakeSpecialDish()
    {
        Console.WriteLine("The Chef makes bbq ribs");
    }
}

public class ItalianChef : Chef
{
    public void MakePasta()
    {
        Console.WriteLine("The Chef makes pasta");
    }

    //Override MakeSpecialDish
    public override void MakeSpecialDish()
    {
        Console.WriteLine("The Chef makes chicken parm");
    }
}

/// <summary>
/// ί�У�����ʱ���Ҫ����Handler
/// </summary>
/// <returns></returns>
public delegate void  TestTimeComingHander(string _contents);

/// <summary>
/// ί�У�����ʱ�䵽��Handler
/// </summary>
public delegate void TestTimeUpHander();

/// <summary>
/// ��ʦ��
/// </summary>
public class Teacher
{
    public event TestTimeComingHander TestTimeComing;

    public event TestTimeUpHander TestTimeUp;

    public Action TestTimeAction;

    /// <summary>
    /// ѧУ��֪ͨ������ʱ��쵽��
    /// </summary>
    /// <param name="_content"></param>
    public void OnTestTimeComing(string _content)
    {
        if (TestTimeComing!=null)
        {
            TestTimeComing(_content);
        }
    }

    /// <summary>
    /// ����ʱ�䵽��
    /// </summary>
    public void OnTestTimeUp()
    {
        if (TestTimeUp!=null)
        {
            TestTimeUp();
        }

        if (TestTimeAction!=null)
        {
            TestTimeAction();
        }
    }

}

/// <summary>
/// ѧ����
/// </summary>
public class ExamStudent
{
    public string Name { get; private set; }

    public ExamStudent(string name)
    {
        this.Name = name;
    }

    public void HandInQingJia(string _contents)
    {
        Console.WriteLine($"{this.Name}\' _contents");
        Console.WriteLine($"{this.Name}\'Ҫ�����ˣ���Ҫ���");
    }

    public void HandInTestPaper()
    {
        Console.WriteLine($"{this.Name}\' paper has been handed in");
    }

}
