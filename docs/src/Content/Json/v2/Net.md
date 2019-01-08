<article class="message is-info">
  <div class="message-body">
<strong>Version 2</strong> of Thoth.Json and Thoth.Json.Net <strong>only support</strong> Fable 2.
  </div>
</article>

# .Net & NetCore support

You can share your decoders and encoders **between your client and server**.

In order to use Thoth.Json API on .Net or NetCore you need to use the `Thoth.Json.Net` package.

## Code sample

```fsharp
// By adding this condition, you can share your code between your client and server
#if FABLE_COMPILER
open Thoth.Json
#else
open Thoth.Json.Net
#endif

type User =
    { Id : int
      Name : string
      Email : string
      Followers : int }

    static member Decoder : Decode.Decoder<User> =
        Decode.object
            (fun get ->
                { Id = get.Required.Field "id" Decode.int
                    Name = get.Optional.Field "name" Decode.string
                            |> Option.defaultValue ""
                    Email = get.Required.Field "email" Decode.string
                    Followers = 0 }
            )

    static member Encoder (user : User) =
        Encode.object
            [ "id", Encode.int user.Id
              "name", Encode.string user.Name
              "email", Encode.string user.Email
              "followers", Encode.int user.Followers
            ]
```
