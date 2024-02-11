// For more information see https://aka.ms/fsharp-console-apps
module Program

open System
open System.Threading
open Akka.Actor
open Akka.FSharp
open System.Net.Http

open HttpRequestSender

let httpClient = new HttpClient()
let cancellationTokenSource  = new CancellationTokenSource()
let system = System.create "system" <| Configuration.defaultConfig()

[<EntryPoint>]
let main _ =
        let httpRequestSenderActor1 = spawn system "http-sender" (actorOf (httpRequestSender(httpClient, cancellationTokenSource, system)))

        httpRequestSenderActor1.Tell <| HttpRequestSenderMessageType.RequestMessage("https://jsonplaceholder.typicode.com/todos/20001", HttpMethod.Get)

        
        let _ = Console.ReadLine()

        0