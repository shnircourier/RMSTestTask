module HttpRequestSender

open System.Net
open System.Threading
open Akka.Actor
open Akka.FSharp
open System.Net.Http
open ConsoleResponseLog

type HttpRequestSenderMessageType =
        | RequestMessage of string * HttpMethod
        | CancelRequestMessage

let httpRequestSender (httpClient: HttpClient, cancellationTokenSource: CancellationTokenSource, system: ActorSystem) (msg: HttpRequestSenderMessageType) =
        match msg with
        | RequestMessage(url, method) ->
                async {
                        try
                                let request = new HttpRequestMessage(method, url)
                                let! response = httpClient.SendAsync(request, cancellationTokenSource.Token) |> Async.AwaitTask
                                let _ = response.EnsureSuccessStatusCode()
                                match response.StatusCode with
                                | HttpStatusCode.OK ->
                                        let! responseBody = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                                        let consoleResponseLogActor = spawn system "log" (actorOf consoleResponseLog)
                                        consoleResponseLogActor.Tell <| responseBody
                                | _ -> failwith $"Status Code is not indicate OK(200). Status Code: {int response.StatusCode}"
                        with
                        | ex -> printfn $"error by making request to {url} with method {method}\n{ex.Message}"
                } |> Async.RunSynchronously
        | CancelRequestMessage -> cancellationTokenSource.Cancel()