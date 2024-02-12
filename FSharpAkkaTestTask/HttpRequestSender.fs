module HttpRequestSender

open System.Threading
open Akka.FSharp
open System.Net.Http
open Microsoft.FSharp.Core

type HttpRequestSenderType =
        | RequestMessage of string * HttpMethod
        | CancelRequestMessage
        
type HttpResponseBodyType = { StatusCode: int; Body: string }

type HttpResponseSenderType =
        | ResponseBody of HttpResponseBodyType
        | CancelRequestBody of string

let httpRequestSender (httpClient: HttpClient, cancellationTokenSource: CancellationTokenSource) (msg: HttpRequestSenderType) =
        match msg with
        | RequestMessage(url, method) ->
                async {
                        try
                                let request = new HttpRequestMessage(method, url)
                                let! response = httpClient.SendAsync(request, cancellationTokenSource.Token) |> Async.AwaitTask
                                let! responseBody = response.Content.ReadAsStringAsync() |> Async.AwaitTask 
                                return HttpResponseSenderType.ResponseBody({ StatusCode = int response.StatusCode; Body = responseBody })
                        with
                        | :? System.Threading.Tasks.TaskCanceledException as _ -> return HttpResponseSenderType.CancelRequestBody(cancellationTokenSource.Token.GetHashCode().ToString())
                } |> Async.RunSynchronously
        | CancelRequestMessage ->
                cancellationTokenSource.Cancel()
                HttpResponseSenderType.CancelRequestBody(cancellationTokenSource.Token.GetHashCode().ToString())