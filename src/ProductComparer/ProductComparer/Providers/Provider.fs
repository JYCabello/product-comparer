module ProductComparer.Providers

open ProductComparer.Models

type ProviderProduct =
  { Barcode: string
    Price: decimal
    ProviderName: string }

type Provider =
  abstract member get: StelProduct -> Async<ProviderProduct option>
  abstract member name: string
