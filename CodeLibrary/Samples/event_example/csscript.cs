using System;
using System.Drawing;
using System.IO;
using System.Data.SQLite;
using System.Data;
using Microsoft.CSharp;
using System.Collections.Generic;
using DevelopWorkspace.Base;
using Excel = Microsoft.Office.Interop.Excel;
using System.Threading;
using System.Threading.Tasks;
public class Script
{
    public static void Main(string[] args)
  {
    Adder a = new Adder();
    a.OnMultipleOfFiveReached += a_MultipleOfFiveReached;
    int iAnswer = a.Add(4, 3);
    Console.WriteLine("iAnswer = {0}", iAnswer);
    iAnswer = a.Add(4, 6);
    Console.WriteLine("iAnswer = {0}", iAnswer);
    Console.ReadKey();
  }

  static void a_MultipleOfFiveReached(object sender, MultipleOfFiveEventArgs e)
  {
    Console.WriteLine("Multiple of five reached: ", e.Total);
  }
}

public class Adder
{
  public event EventHandler<MultipleOfFiveEventArgs> OnMultipleOfFiveReached;
  public int Add(int x, int y)
  {
    int iSum = x + y;
    if ((iSum % 5 == 0) && (OnMultipleOfFiveReached != null))
    { OnMultipleOfFiveReached(this, new MultipleOfFiveEventArgs(iSum)); }
    return iSum;
  }
}

public class MultipleOfFiveEventArgs : EventArgs
{
  public MultipleOfFiveEventArgs(int iTotal)
  { Total = iTotal; }
  public int Total { get; set; }
}

/*
public event EventHandler MyLongRunningTaskEvent;

private void StartMyLongRunningTask() {
    MyLongRunningTaskEvent += myLongRunningTaskIsDone;
    Thread _thread = new Thread(myLongRunningTask) { IsBackground = true };
    _thread.Start();
    label.Text = "Running...";
}

private void myLongRunningTaskIsDone(object sender, EventArgs arg)
{
    label.Text = "Done!";
}

private void myLongRunningTask()
{
    try 
    { 
        // Do my long task...
    } 
    finally
    {
        this.BeginInvoke(Foo, this, EventArgs.Empty);
    }
}
*/
//使用threadpool
/*
    class Program
    {
        static void Main(string[] args)
        {


            Stopwatch mywatch = new Stopwatch();

            Console.WriteLine("Thread Pool Execution");

            int workerThreads, completionPortThreads;
            ThreadPool.GetMaxThreads(out workerThreads, out completionPortThreads);
            workerThreads = 10;
            ThreadPool.SetMaxThreads(workerThreads, completionPortThreads);


            mywatch.Start();
            ProcessWithThreadPoolMethod();
            mywatch.Stop();

            Console.WriteLine("Time consumed by ProcessWithThreadPoolMethod is : " + mywatch.ElapsedTicks.ToString());
            mywatch.Reset();


            Console.WriteLine("Thread Execution");

            mywatch.Start();
            ProcessWithThreadMethod();
            mywatch.Stop();

            Console.WriteLine("Time consumed by ProcessWithThreadMethod is : " + mywatch.ElapsedTicks.ToString());
            Console.ReadLine();
        }

        static void ProcessWithThreadPoolMethod()
        {
            for (int i = 0; i <= 10; i++)
            {

                ThreadPool.QueueUserWorkItem(new WaitCallback(Process),2);
            }
        }

        static void ProcessWithThreadMethod()
        {
            for (int i = 0; i <= 10; i++)
            {
                Thread obj = new Thread(Process);
                obj.Start(1);
            }
        }

        static void Process(object callback)
        {
            Console.WriteLine("callback:" + callback == null?"pool":callback.ToString() + "####" +Thread.CurrentThread.ManagedThreadId);
            Thread.Sleep(1000);
        }


    }

*/
/*
public void wireupBackGroundWorker() {

            //note in .net 4.0 include this using statement "using System.ComponentModel;"

            BackgroundWorker bgw = new BackgroundWorker();

            bgw.ProgressChanged += new ProgressChangedEventHandler(bgw_ProgressChanged);

            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_RunWorkerCompleted);

            bgw.WorkerReportsProgress = true;

            bgw.DoWork += new DoWorkEventHandler(bgw_DoWork); 

            //to call

            object argument = "TESTING";

            bgw.RunWorkerAsync(argument);

        }

 

        void bgw_DoWork(object sender, DoWorkEventArgs e)

        {

            //this method is running on a separate thread you cannot update gui here

            string argument = e.Argument as string;

            //do long outstanding process here...

            //if you decalre bgw globally you can update progress here

            bgw.ReportProgress(90);  //90% done

            //or you can pass an object too

            object userState = "Taking a bit longer than expected";

            bgw.ReportProgress(80, userState);

           

           

        }

 

        void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)

        {

            object result = e.Result;

            //now you are back on main thread and can update GUI

        }

 

        void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)

        {

            object percentage = e.ProgressPercentage;           

            //on main thread can update gui          

           

        }
 */