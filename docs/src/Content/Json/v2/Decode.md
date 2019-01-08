<article class="message is-info">
  <div class="message-body">
<strong>Version 2</strong> of Thoth.Json and Thoth.Json.Net <strong>only support</strong> Fable 2.
  </div>
</article>

# Decode

Turn JSON values into F# values.

By using a Decoder, you will be guaranteed that the JSON structure is correct.
This is especially useful if you use Fable without sharing your domain with the server.

*This module is inspired by [Json.Decode from Elm](http://package.elm-lang.org/packages/elm-lang/core/latest/Json-Decode)
and [elm-decode-pipeline](http://package.elm-lang.org/packages/NoRedInk/elm-decode-pipeline/latest).*

As so, to complete this documentation, you can also take a look at the [Elm documentation](https://guide.elm-lang.org/interop/json.html).

## What is a Decoder ?

Here is the signature of a `Decoder`:

```fsharp
type Decoder<'T> = string -> obj -> Result<'T, DecoderError>
```

This is taking two arguments:

- the traveled path
- an "untyped" value and checking if it has the expected structure.

If the structure is correct, then you get an `Ok` result, otherwise an `Error` explaining why and where the decoder failed.

Example of error:

```fsharp
Error at: `$.user.firstname`
Expecting an object with path `user.firstname` but instead got:
{
    "user": {
        "name": "maxime",
        "age": 25
    }
}
Node `firstname` is unkown.
```

The path generated is a valid `JSONPath`, so you can use tools like [JSONPath Online Evaluator](http://jsonpath.com/) to explore your JSON.

## Primitives decoders

- `string : Decoder<string>`
- `guid : Decoder<System.Guid>`
- `int : Decoder<int>`
- `int64 : Decoder<int64>`
- `uint64 : Decoder<uint64>`
- `bigint : Decoder<bigint>`
- `bool : Decoder<bool>`
- `float : Decoder<float>`
- `decimal : Decoder<decimal>`
- `datetime : Decoder<System.DateTime>`
- `datetimeOffset : Decoder<System.DateTimeOffset>`

```fsharp
open Thoth.Json

> Decode.fromString Decode.string "\"maxime\""
val it : Result<string, string> = Ok "maxime"

> Decode.fromString Decode.int "25"
val it : Result<int, string> = Ok 25

> Decode.fromString Decode.bool "true"
val it : Result<bool, string> = Ok true

> Decode.fromString Decode.float "true"
val it : Result<float, string> = Error "Error at: `$`\n Expecting a float but instead got: true"
```

With these primitives decoders we can handle the basic JSON values.

## Collections

There are special decoders for the following collections.

- `list : Decoder<'value> -> Decoder<'value list>`
- `array : Decoder<'value> -> Decoder<'value array>`
- `index : -> int -> Decoder<'value> -> Decoder<'value>`

```fsharp
open Thoth.Json

> Decode.fromString (array int) "[1, 2, 3]"
val it : Result<int [], string> =  Ok [|1, 2, 3|]

> Decode.fromString (list string) """["Maxime", "Alfonso", "Vesper"]"""
val it : Result<string list, string> = Ok ["Maxime", "Alfonso", "Vesper"]

> Decode.fromString (Decode.index 1 Decode.string) """["maxime", "alfonso", "steffen"]"""
val it : Result<string, string> = Ok("alfonso")
```

## Decoding Objects

In order to decode objects, you can use:

- `field : string -> Decoder<'value> -> Decoder<'value>`
    - Decode a JSON object, requiring a particular field.
- `at : string list -> Decoder<'value> -> Decoder<'value>`
    - Decode a JSON object, requiring certain path.

```fsharp
open Thoth.Json

> Decode.fromString (field "x" int) """{"x": 10, "y": 21}"""
val it : Result<int, string> = Ok 10

> Decode.fromString (field "y" int) """{"x": 10, "y": 21}"""
val it : Result<int, string> = Ok 21
```

**Important:**

These two decoders only take into account the provided field or path. The object can have other fields/paths with other content.

### Map functions

To get data from several fields and convert them into a record you will need to use the `map` functions
like `map2`, `map3`, ..., `map8`.

```fsharp
open Thoth.Json

type Point =
    { X : int
      Y : int }

    static member Decoder : Decode.Decoder<Point> =
        Decode.map2 (fun x y ->
                { X = x
                  Y = y } : Point)
             (Decode.field "x" Decode.int)
             (Decode.field "y" Decode.int)

> Decode.fromString Point.Decoder """{"x": 10, "y": 21}"""
val it : Result<Point, string> = Ok { X = 10; Y = 21 }
```

### Object builder style

When working with a larger object, you can use the object builder helper.

```fsharp
open Thoth.Json

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

> Decode.fromString User.Decoder """{ "id": 67, "email": "user@mail.com" }"""
val it : Result<User, string> = Ok { Id = 67; Name = ""; Email = "user@mail.com"; Followers = 0 }
```

### Auto decoder

If your JSON structure is a one to one match, with your F# type, you can use auto decoders. Auto decoders, will generate the decoder at runtime for you and still guarantee that the JSON structure is correct.

```fsharp
> let json = """{ "Id" : 0, "Name": "maxime", "Email": "mail@domain.com", "Followers": 0 }"""
> Decode.Auto.fromString<User>(json)
val it : Result<User, string> = Ok { Id = 0; Name = "maxime"; Email = "mail@domain.com"; Followers = 0 }
```

Auto decoder accept an optional argument `isCamelCase`:
- if `true`, then the keys in the JSON are considered `camelCase`
- if `false`, then the keys in the JSON are considered `PascalCase`

```fsharp
> let json = """{ "id" : 0, "name": "maxime", "email": "mail@domain.com", "followers": 0 }"""
> Decode.Auto.fromString<User>(json, isCamelCase=true)
val it : Result<User, string> = Ok { Id = 0; Name = "maxime"; Email = "mail@domain.com"; Followers = 0 }
```

If you prefer not to deal with a `Result<'T, string>` type you can use `Decode.Auto.unsafeFromString`.
- if the decoder succeed, it returns `'T`.
- if the decoder failed, it will throw an exception with the explanation in the `Message` property.

## Size optimization

Note auto decoders use reflection info, which in Fable 2 is generated in the call site. If you want to save some bytes in the generated JS code, it's recommended to cache decoders instead of using `Decode.Auto.fromString` directly.

```fsharp
// Instead of:
let method1 json =
    Decode.Auto.fromString<Foo> json

let method2 json =
    Decode.Auto.fromString<Foo> json

// Do this:
let fooDecoder = Decode.Auto.generateDecoder<Foo>()

let method1 json =
    Decode.fromString fooDecoder json

let method2 json =
    Decode.fromString fooDecoder json
```

For similar reasons, when possible it's better to compose decoders instead of generating them automatically.

```fsharp
// Instead of:
type Group = { foo: Foo; bar: Bar }

let fooDecoder = Decode.Auto.generateDecoder<Foo>()
let barDecoder = Decode.Auto.generateDecoder<Bar>()

let fooListDecoder = Decode.Auto.generateDecoder<Foo list>()
let groupDecoder = Decode.Auto.generateDecoder<Group>()

// Do this:
let fooListDecoder: Decode.Decoder<Foo list> =
    Decode.list fooDecoder

let groupDecoder: Decode.Decoder<Group> =
    Decode.object (fun get ->
        { foo = get.Required.Field "foo" fooDecoder
          bar = get.Required.Field "bar" barDecoder })
```
