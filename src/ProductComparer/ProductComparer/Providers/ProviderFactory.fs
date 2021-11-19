module ProductComparer.ProviderFactory

open ProductComparer.Providers
open ProductComparer.Providers_Infortisa

let providers: Provider list = [ Infortisa() ]
