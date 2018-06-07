module Thoth.Json.Net.Encode

open Newtonsoft.Json
open Newtonsoft.Json.Linq
open System.IO

type Replacer = string -> obj -> obj

///**Description**
/// Encode a string
///
///**Parameters**
///  * `value` - parameter of type `string`
///
///**Output Type**
///  * `Value`
///
///**Exceptions**
///
let string (value : string) : JToken =
    JValue(value) :> JToken

///**Description**
/// Encode an int
///
///**Parameters**
///  * `value` - parameter of type `int`
///
///**Output Type**
///  * `Value`
///
///**Exceptions**
///
let int (value : int) : JToken =
    JValue(value) :> JToken

///**Description**
/// Encode a Float. `Infinity` and `NaN` are encoded as `null`.
///
///**Parameters**
///  * `value` - parameter of type `float`
///
///**Output Type**
///  * `Value`
///
///**Exceptions**
///
let float (value : float) : JToken =
    JValue(value) :> JToken

///**Description**
/// Encode null
///
///**Parameters**
///
///**Output Type**
///  * `Value`
///
///**Exceptions**
///
let nil : JToken =
    JValue(box null) :> JToken

///**Description**
/// Encode a bool
///**Parameters**
///  * `value` - parameter of type `bool`
///
///**Output Type**
///  * `Value`
///
///**Exceptions**
///
let bool (value : bool) : JToken =
    JValue(value) :> JToken

///**Description**
/// Encode an object
///
///**Parameters**
///  * `values` - parameter of type `(string * Value) list`
///
///**Output Type**
///  * `Value`
///
///**Exceptions**
///
let object (values : (string * JToken) list) : JToken =
    values
    |> List.map (fun (key, value) ->
        JProperty(key, value)
    )
    |> JObject :> JToken

///**Description**
/// Encode an array
///
///**Parameters**
///  * `values` - parameter of type `Value array`
///
///**Output Type**
///  * `Value`
///
///**Exceptions**
///
let array (values : array<JToken>) : JToken =
    JArray(values) :> JToken

///**Description**
/// Encode a list
///**Parameters**
///  * `values` - parameter of type `Value list`
///
///**Output Type**
///  * `Value`
///
///**Exceptions**
///
let list (values : JToken list) : JToken =
    JArray(values) :> JToken

///**Description**
/// Encode a dictionary
///**Parameters**
///  * `values` - parameter of type `Map<string, Value>`
///
///**Output Type**
///  * `Value`
///
///**Exceptions**
///
let dict (values : Map<string, JToken>) =
    values
    |> Map.toList
    |> object

///**Description**
/// Convert a `Value` into a prettified string.
///**Parameters**
///  * `space` - parameter of type `int` - Amount of indentation
///  * `value` - parameter of type `obj` - Value to convert
///
///**Output Type**
///  * `string`
///
///**Exceptions**
///
let encode (space: int) (token: JToken) : string =
    let format = if space = 0 then Formatting.None else Formatting.Indented
    use stream = new StringWriter(NewLine = "\n")
    use jsonWriter = new JsonTextWriter(
                            stream,
                            Formatting = format,
                            Indentation = space )

    token.WriteTo(jsonWriter)
    stream.ToString()

let encodeAuto (space: int) (value: obj) : string =
    // TODO: Can we set indentation space?
    let format = if space = 0 then Formatting.None else Formatting.Indented
    let settings = JsonSerializerSettings(Converters = [|Converters.CacheConverter.Singleton|],
                                          Formatting = format)
    JsonConvert.SerializeObject(value, settings)

///**Description**
/// Encode an option
///**Parameters**
///  * `encoder` - parameter of type `'a -> Value`
///
///**Output Type**
///  * `'a option -> Value`
///
///**Exceptions**
///
let option (encoder : 'a -> JToken) =
    Option.map encoder >> Option.defaultWith (fun _ -> nil)
