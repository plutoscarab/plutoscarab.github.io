namespace PlutoScarab

open System
open PlutoScarab.Parser

module Json =

    // JSON data model

    type Json =
        | JNull
        | JBool of bool
        | JNumber of double // I-JSON
        | JString of string
        | JArray of Json list
        | JObject of (string * Json) list


    // Do required escaping of special characters in a string

    let private jstr s =
        let escape c =
            match c with
            | '\r' -> "\\r"
            | '\n' -> "\\n"
            | '\t' -> "\\t"
            | '\b' -> "\\b"
            | '\f' -> "\\f"
            | '"'
            | '\\' -> "\\" + (string c)
            | ch when ch < ' ' -> $"\\u{(int ch):X4}"
            | _ -> string c

        "\""
        + (s |> Seq.map escape |> String.Concat)
        + "\""


    // Convert a Json to a string

    let rec toStr json =
        match json with
        | JNull -> "null"
        | JBool b -> if b then "true" else "false"
        | JNumber n -> n.ToString()
        | JString s -> jstr s
        | JArray arr ->
            "["
            + (String.concat ", " (List.map toStr arr))
            + "]"
        | JObject obj ->
            let memberStr (key, value) = (jstr key) + ":" + (toStr value)

            "{"
            + (String.concat ", " (List.map memberStr obj))
            + "}"


    // Convert a Json to an indented string

    let rec private toIndentedN json oldIndent =
        let indent = oldIndent + "  "

        match json with
        | JNull -> "null"
        | JBool b -> if b then "true" else "false"
        | JNumber n -> n.ToString()
        | JString s -> jstr s
        | JArray arr ->
            "[\n"
            + indent
            + (String.concat (",\n" + indent) (arr |> List.map (fun v -> toIndentedN v indent)))
            + "\n"
            + oldIndent
            + "]"
        | JObject obj ->
            let memberStr (key, value) =
                (jstr key) + ": " + (toIndentedN value indent)

            "{\n"
            + indent
            + (String.concat (",\n" + indent) (List.map memberStr obj))
            + "\n"
            + oldIndent
            + "}"

    let toIndented json = toIndentedN json ""


    let jnull = 
        str "n" &. str "ull" 
        |> anyFatal 
        |> capture (fun _ -> JNull)


    let jfalse =
        str "f" &. str "alse"
        |> anyFatal
        |> capture (fun _ -> JBool false)


    let jtrue =
        str "t" &. str "rue"
        |> anyFatal
        |> capture (fun _ -> JBool true)


    let digit<'a> = '0' -- '9'

    let jnumber =
        %(str "-")
        &. (str "0" |. ('1' -- '9' &. -.digit))
        &. %(str "." &. +.digit)
        &. %(oneOf "Ee" &. %(oneOf "+-") &. +.digit)
        |> anyFatal
        |> capture (fun s -> JNumber(Double.Parse(s)))


    let hexChar<'a> = oneOf "0123456789AaBbCcDdEeFf"

    let hiSurrogateCode =
        oneOf "Dd" &. oneOf "89AaBb" &. hexChar &. hexChar

    let loSurrogateCode =
        oneOf "Dd" &. oneOf "CcDdEeFf" &. hexChar &. hexChar

    let nonSurrogateCode =
        ((oneOf "0123456789AaBbCcEeFf" &. hexChar) // 00 to CF, or E0 to FF
         |. (oneOf "Dd" &. oneOf "01234567")) // D0 to D7
        &. hexChar
        &. hexChar

    let codeUnit (s: string) = char (Convert.ToInt32(s, 16))

    let hiSurrogate =
        // non-escaped
        ('\uD800' -- '\uDBFF' |> capture (fun s -> s.[0]))
        // escaped
        |. (str "\\u" &. (hiSurrogateCode |> capture codeUnit))

    let loSurrogate =
        // non-escaped
        ('\uDB00' -- '\uDFFF' |> capture (fun s -> s.[0]))
        // escaped
        |. (str "\\u" &. (loSurrogateCode |> capture codeUnit))

    let surrogatePair = 
        hiSurrogate &. loSurrogate

    let nonSurrogate =

        // non-escaped, non-control, and not quote (22) or backslash (5C)
        (('\u0020' -- '\u0021'
          |. '\u0023' -- '\u005B'
          |. '\u005D' -- '\uD7FF'
          |. '\uE000' -- '\uFFFF')
         |> capture (fun s -> s.[0]))

        // escaped
        |. (str "\\" &.

            // escaped by codepoint value
            ((str "u" &. (nonSurrogateCode |> capture codeUnit))

            // escaped by special combo
            |. (str "t" |> capture (fun s -> '\t'))
            |. (str "r" |> capture (fun s -> '\r'))
            |. (str "n" |> capture (fun s -> '\n'))
            |. (str "b" |> capture (fun s -> '\b'))
            |. (str "f" |> capture (fun s -> '\f'))
            |. (str "\"" |> capture (fun s -> '"'))
            |. (str "\\" |> capture (fun s -> '\\'))
            |. (str "/" |> capture (fun s -> '/'))))

    let jstring =
        str "\""
        &. -.(surrogatePair |. nonSurrogate)
        &. str "\""
        |> anyFatal
        |> map (fun cs -> [ JString(String.Concat cs) ])


    let whitespace = -.(oneOf " \t\r\n")

    let rec valueSelector (c : Cursor) =
        match c.Head with
        | Some 'n' -> jnull c
        | Some 'f' -> jfalse c
        | Some 't' -> jtrue c
        | Some ch when Char.IsDigit(ch) -> jnumber c
        | Some '-' -> jnumber c
        | Some '"' -> jstring c
        | Some '[' -> jarray c
        | Some '{' -> jobject c
        | _ -> Err("one of n, f, t, -, \", [, {, or digit", c.index)

    and jvalue =
        whitespace &. valueSelector &. whitespace

    and jarray =
        ((str "["
          &. %(jvalue &. -.(str "," &. jvalue))
          &. whitespace
          &. str "]")
         |> anyFatal
         |> map (fun list -> [ JArray list ]))

    and jobject =

        let field =
            whitespace &. jstring &. whitespace &. str ":" &. jvalue &. whitespace
            |> map (fun [ JString name; value ] -> [ name, value ])

        (str "{"
         &. -.(oneOf " \t\r\n")
         &. %(field &. -.(str "," &. field |> anyFatal))
         &. str "}")
        |> anyFatal
        |> map (fun list -> [ JObject list ])


    let jparser s =

        let result = jvalue { text = s; index = 0 }

        match result with
        | Ok (_, _, c) when c.index <> s.Length -> Err("end of text", c.index)
        | _ -> result
