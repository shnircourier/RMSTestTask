module HttpRequestSenderUnitTest

open System.Net.Http
open System.Threading
open NUnit.Framework
open HttpRequestSender

[<SetUp>]
let Setup () =
    ()

[<TestFixture>]
type HttpRequestSenderTests() =

    let httpClient = new HttpClient()
    let cancellationTokenSource  = new CancellationTokenSource()
    
    [<Test>]
    member this.``http request sender should return response type with body and 200 code``() =
        // Arrange
        let expectedOutput = {StatusCode = 200; Body = "{\"userId\": 1,\"id\": 1,\"title\": \"delectus aut autem\",\"completed\": false}" }
        let method = HttpMethod.Get
        let url = "https://jsonplaceholder.typicode.com/todos/1"
        let output = httpRequestSender(httpClient, cancellationTokenSource) (HttpRequestSenderType.RequestMessage(url, method))
        
        // Assert
        match output with
        | ResponseBody body ->
            Assert.AreEqual(expectedOutput.StatusCode, body.StatusCode)
            Assert.AreEqual(expectedOutput.Body.Replace("\n", "").Replace(" ", ""), body.Body.Replace("\n", "").Replace(" ", ""))
        | CancelRequestBody _ ->
            Assert.Fail()
    
    [<Test>]
    member this.``http request sender should stop when interrupt``() =
         // Arrange
         let method = HttpMethod.Get
         let url = "https://jsonplaceholder.typicode.com/todos/20001"
         let sendInterruption = httpRequestSender(httpClient, cancellationTokenSource) HttpRequestSenderType.CancelRequestMessage
         let output = httpRequestSender(httpClient, cancellationTokenSource) (HttpRequestSenderType.RequestMessage(url, method))
    
         // Assert
         match output with
         | ResponseBody _ ->
            Assert.Fail()
         | CancelRequestBody response ->
            Assert.AreEqual(cancellationTokenSource.Token.GetHashCode().ToString(), response)
    
    [<Test>]
    member this.``http request sender should return 404 status code``() =
        // Arrange
        let expectedStatusCode = 404
        let method = HttpMethod.Get
        let url = "https://jsonplaceholder.typicode.com/todos/20001"
        let output = httpRequestSender(httpClient, cancellationTokenSource) (HttpRequestSenderType.RequestMessage(url, method))
        
        // Assert
        match output with
        | ResponseBody body ->
            Assert.AreEqual(expectedStatusCode, body.StatusCode)
        | CancelRequestBody _ ->
            Assert.Fail()