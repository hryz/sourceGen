using System;
using System.Threading.Tasks;
using zipkin4net;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Transport.Http;

namespace net5
{
    public class ZipkinFun
    {
        class ConsoleLogger : ILogger
        {
            public void LogInformation(string message) => Console.WriteLine("INF: " + message);
            public void LogWarning(string message) => Console.WriteLine("WRN: " + message);
            public void LogError(string message) => Console.WriteLine("ERR: " + message);
        }
        
        public static async Task RunDemo()
        {
            var sender = new HttpZipkinSender("http://localhost:9411", "application/json");
            var serializer = new JSONSpanSerializer();
            var tracer = new ZipkinTracer(sender, serializer);
            var logger = new ConsoleLogger();
            
            TraceManager.SamplingRate = 1.0f; //full tracing
            TraceManager.RegisterTracer(tracer);
           
            TraceManager.Start(logger); //on startup

            var trace = Trace.Create();

            await LinePopulator.NewUpdate(trace);

            TraceManager.Stop();//On shutdown
        }
        
        
        public class LinePopulator
        {
            public static async Task NewUpdate(Trace trace)
            {
                trace.Record(Annotations.ConsumerStart()); //start processing
                trace.Record(Annotations.ServiceName("Line Populator"));
                trace.Record(Annotations.Rpc("Probability changed"));
                await Task.Delay(1000);
                trace.Record(Annotations.ConsumerStop()); //end processing
                await Task.WhenAll(
                    OddsMiddleware.NewUpdate(trace),
                    GlobalEtl.NewUpdate(trace)); //produce to kafka downstream
            }
        }

        class OddsMiddleware
        {
            public static async Task NewUpdate(Trace trace)
            {
                await Task.Delay(300); //kafka delay
                trace = trace.Child(); //create a new span with parent span id and trace id passed in headers
                trace.Record(Annotations.ConsumerStart()); //start processing 
                trace.Record(Annotations.ServiceName("Odds Middleware"));
                trace.Record(Annotations.Rpc("LineCalculation changed"));
                await Task.Delay(1500);// hard work
                trace.Record(Annotations.ConsumerStop()); //end processing 
                await OperatorEtl.NewUpdate(trace); //sent downstream
            }
        }

        class GlobalEtl
        {
            public static async Task NewUpdate(Trace trace)
            {
                await Task.Delay(400); //kafka delay
                trace = trace.Child();
                trace.Record(Annotations.ConsumerStart()); //start processing 
                trace.Record(Annotations.ServiceName("Global ETL"));
                trace.Record(Annotations.Rpc("LineCalculation changed"));
                await Task.Delay(700);// hard work
                trace.Record(Annotations.ConsumerStop()); //end processing
                await OperatorEtl.NewUpdate(trace); //sent downstream
            }
        }

        class OperatorEtl
        {
            public static async Task NewUpdate(Trace trace)
            {
                await Task.Delay(200); //kafka delay
                trace = trace.Child();
                trace.Record(Annotations.ConsumerStart()); //start processing
                trace.Record(Annotations.ServiceName("Operator ETL"));
                trace.Record(Annotations.Rpc("Selection changed"));
                await Task.Delay(1500);// hard work
                trace.Record(Annotations.ConsumerStop()); //end processing
                await Api.NewUpdate(trace); //send downstream
            }
        }

        class Api
        {
            public static async Task NewUpdate(Trace trace)
            {
                await Task.Delay(300); //kafka delay
                trace = trace.Child();
                trace.Record(Annotations.ConsumerStart()); //start processing
                trace.Record(Annotations.ServiceName("API"));
                trace.Record(Annotations.Rpc("Selection changed"));
                await Task.Delay(300); // hard work
                trace.Record(Annotations.ConsumerStop()); //end processing
            }
        }
    }
    
    
}