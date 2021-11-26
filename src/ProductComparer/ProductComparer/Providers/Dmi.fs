module ProductComparer.Providers_Dmi

open FsToolkit.ErrorHandling
open ProductComparer.Singletons

type InfortisaCredentials = { Username: string; Password: string }


let credentials =
  option {
    let! userName = settings.DmiUsername
    let! password = settings.DmiPassword
    return { Username = userName; Password = password }
  }
