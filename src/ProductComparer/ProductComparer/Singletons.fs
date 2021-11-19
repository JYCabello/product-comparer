module ProductComparer.Singletons

open System
open System.Net.Http

let httpClient =
  let client = new HttpClient()
  client.Timeout <- TimeSpan.FromMinutes 5.0
  client
