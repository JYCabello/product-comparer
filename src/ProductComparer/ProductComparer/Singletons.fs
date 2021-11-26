module ProductComparer.Singletons

open System
open System.IO
open System.Net.Http
open Newtonsoft.Json
open ProductComparer.Models

let httpClient =
  let client = new HttpClient()
  client.Timeout <- TimeSpan.FromMinutes 5.0
  client

let settings =
  JsonConvert.DeserializeObject<SettingsDto>(File.ReadAllText("appsettings.json"))
  |> Settings.from
