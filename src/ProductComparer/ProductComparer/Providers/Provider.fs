module ProductComparer.Providers

open ProductComparer.Models

type Provider =
  abstract member get: StelProduct -> Async<ProviderProduct option>
  abstract member name: string
