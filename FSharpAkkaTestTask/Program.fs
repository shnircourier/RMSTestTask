// For more information see https://aka.ms/fsharp-console-apps
open System
open System.Net
open System.Threading
open Akka.Actor
open Akka.FSharp
open System.Net.Http

type HttpRequestSenderMessageType =
        | RequestMessage of string * HttpMethod
        | CancelRequestMessage

let httpClient = new HttpClient()
let cancellationTokenSource  = new CancellationTokenSource()
let system = System.create "system" <| Configuration.defaultConfig()

let consoleResponseLog (msg: obj) =
        match msg with
        | :? string as responseBody -> printfn $"Response body:\n{responseBody}"
        | _ -> printfn "response body is not string"
        
let httpRequestSender (msg: HttpRequestSenderMessageType) =
        match msg with
        | RequestMessage(url, method) ->
                async {
                        try
                                let request = new HttpRequestMessage(method, url)
                                let! response = httpClient.SendAsync(request, cancellationTokenSource.Token) |> Async.AwaitTask
                                let! responseBody = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                                match response.StatusCode with
                                | HttpStatusCode.OK ->
                                        let consoleResponseLogActor = spawn system "log" (actorOf consoleResponseLog)
                                        consoleResponseLogActor.Tell <| responseBody
                                | _ -> failwith $"Status Code is not indicate OK(200). Status Code: {int response.StatusCode}"
                        with
                        | ex -> printfn $"error by making request to {url} with method {method}\n{ex.Message}"
                } |> Async.RunSynchronously
        | CancelRequestMessage -> cancellationTokenSource.Cancel()
let httpRequestSenderActor = spawn system "http-sender" (actorOf httpRequestSender)

httpRequestSenderActor.Tell <| HttpRequestSenderMessageType.RequestMessage("https://jsonplaceholder.typicode.com/todos/1", HttpMethod.Get)

let _ = Console.ReadLine()