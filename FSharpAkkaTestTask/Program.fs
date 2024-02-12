// For more information see https://aka.ms/fsharp-console-apps
module Program

open System
open System.Threading
open Akka.FSharp
open System.Net.Http

open HttpRequestSender
open ConsoleResponseLog

[<EntryPoint>]
let main _ =
        let httpClient = new HttpClient()
        let cancellationTokenSource  = new CancellationTokenSource()
        let system = System.create "system" <| Configuration.defaultConfig()
        
        let loggerActor = spawn system "logger" (actorOf consoleResponseLog)
        
        let httpActor = spawn system "http"
                        <| fun mailbox ->
                            let rec loop() = actor {
                                let! message = mailbox.Receive()
                                mailbox.Sender() <! httpRequestSender(httpClient, cancellationTokenSource) message
                                return! loop()
                            }
                            loop()
                            
        async {
            let! httpResponse = httpActor <? HttpRequestSenderType.RequestMessage("https://jsonplaceholder.typicode.com/todos/1", HttpMethod.Get)
            match httpResponse with
            | ResponseBody body -> if body.StatusCode = 200 then loggerActor <! body.ResponseBody
            | CancelRequestBody id -> printfn $"Request with id: {id} stopped!"
        } |> Async.RunSynchronously
        
        let _ = Console.ReadLine()

        0