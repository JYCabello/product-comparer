module ProductComparer.ProviderFactory

open ProductComparer.Providers
open ProductComparer.Providers_Dmi
open ProductComparer.Providers_Infortisa

let providers: Provider list = [ Infortisa(); Dmi() ]
