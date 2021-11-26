module ProductComparer.Providers

open ProductComparer.Models

type Provider =
  abstract member get: OwnProduct -> Async<ProviderProduct option>
  abstract member name: string
