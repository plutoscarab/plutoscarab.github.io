---
title: GeoJson and JSON in F#
tags: GIS, JSON, F#
---

I just started learning [F#](https://fsharp.org/). I learn new programming languages best when I do
something useful with them, and I decided to make a [GeoJson](https://tools.ietf.org/html/rfc7946)
parser. And I don't mean make a GeoJson grammar using [FParsec](https://www.quanttec.com/fparsec/)
or some other library, I mean do the whole thing: make as much of FParsec as I need myself.

I wanted to be sure to do the Unicode stuff right, but by including it here I risk obscuring the
interesting bits by including a bunch of side quests. But who knows, maybe you came here for the
Unicode stuff!

There's also a lot of stuff for testing in here, which might bore you, but if you're not writing the
tests then you're doing it wrong. I decided to just dump that stuff in here in case anyone finds it
interesting.

This isn't necessarily the _best_ way to do parsing, but it was a fun project and was good enough to
start with.

## JSON Model

GeoJson is [JSON](https://tools.ietf.org/html/rfc7159), so first we need a JSON parser, and before
that we need to be able to model JSON data. I'm going to do sort-of
[I-JSON](https://tools.ietf.org/html/rfc7493) to make things slightly easier, which means only doing
float64-compatible numbers, and no invalid surrogate sequences in strings. I-JSON also wants unique
member names in objects, but I have some use cases for duplicate member names so I'm not going to do
that part, and it makes it easier.

Here's the JSON model in F#:

```fsharp
// JSON data model

type Json =
    | JNull
    | JBool of bool
    | JNumber of double // I-JSON
    | JString of string
    | JArray of Json list
    | JObject of (string * Json) list
```

That's it. Imagine what this would look like in C#. You'd probably have an abstract base class and
six derived classes (or a base interface and six implementations), each with constructors and three
of them with property accessors, and if you want immutability (which I do!) you have to do a bunch
of extra work. [C#
records](https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/exploration/records) make this a
lot better, but still nowhere near as nice as this. Records would be a problem here because there's
no inheritance for records, and you really want a base "value" type to use in arrays and objects, so
you'd need to define an interface.

To define data using this model will usually require some code to convert from some other model
to this model, and I want to get into that in the future. But for now, here's how you would use
this to define a GeoJson point feature:

```fsharp
// GeoJson feature using the JSON model

let tourEiffel =
    JObject [
        "type", JString "Feature";
        "id",   JString "a7931925-795e-4b5a-86a8-e19bc5578830";
        "geometry", JObject [
            "type", JString "Point";
            "coordinates", JArray [ JNumber 2.2949378; JNumber 48.858242 ];
        ];
        "properties", JObject [
            "type",     JString "observation tower";
            "location", JString "7th arrondissement, Paris, France";
            "url",      JString "https://www.toureiffel.paris/";
        ];
    ]
```

This is just to show what this almost-JSON looks like in F#. Later we'll make a GeoJson model like
we did for JSON, and then automate the conversion from the GeoJson model to the JSON model.

## JSON Text Representation

Next we need to be able to generate the JSON text representation of a Json. We can do this easily.
Here's a version that doesn't add any whitespace.

```fsharp
// Convert a Json to a string

let rec toStr json =
    match json with
    | JNull -> 
        "null"
    | JBool b -> 
        if b then "true" else "false"
    | JNumber n -> 
        n.ToString()
    | JString s -> 
        jstr s
    | JArray arr -> 
        "[" + (String.concat "," (List.map toStr arr)) + "]"
    | JObject obj ->
        let memberStr (key, value) = (jstr key) + ":" + (toStr value)
        "{" + (String.concat "," (List.map memberStr obj)) + "}"
```

It's recursive because it calls itself in the array and object cases. It uses a helper function
called `jstr` to handle the character escaping requirements for strings:

```fsharp
// Do required escaping of special characters in a string

let jstr s =
    let escape c = 
        match c with
        | '\r' -> "\\r"
        | '\n' -> "\\n"
        | '\t' -> "\\t"
        | '\b' -> "\\b"
        | '\f' -> "\\f"
        | '"' | '\\' -> "\\" + (string c)
        | ch when ch < ' ' -> $"\\u{(int ch):X4}"
        | _ -> string c
    "\"" + (s |> Seq.map escape |> String.Concat) + "\""
```

I love how concise all of this is, but still easy to follow. Here's a version that adds whitespace
for indentation. It's almost identical to the basic version; only the array and object cases needed
to be changed, and only slightly.

```fsharp
// Convert a Json to an indented string

let rec private toIndentedN json oldIndent =
    let indent = oldIndent + "  "
    match json with
    | JNull -> 
        "null"
    | JBool b -> 
        if b then "true" else "false"
    | JNumber n -> 
        n.ToString()
    | JString s -> 
        jstr s
    | JArray arr ->
        "[\n" + indent 
        + (String.concat (",\n" + indent) (arr |> List.map (fun v -> toIndentedN v indent))) 
        + "\n" + oldIndent + "]"
    | JObject obj ->
        let memberStr (key, value) = (jstr key) + ": " + (toIndentedN value indent)
        "{\n" + indent + (String.concat (",\n" + indent) (List.map memberStr obj)) + "\n" + oldIndent + "}"

let toIndented json = 
    toIndentedN json ""
```

## Generating Test Data

For testing purposes, let's generate thousands of random `Json` values, convert to text, and find
unique ones to test our parser with. This isn't purely functional code because it uses mutation in
the standard .NET random number generator, but that's okay for testing.

For numbers, we want some positive and some negative, some with fractional parts and some without,
with varying numbers of digits, and some with exponent parts and some without. This does the trick:

```fsharp
// Generate a random number for JSON testing

let randomNumber (rand : Random) =
    let x = Math.Round(rand.NextDouble() - 0.5, 4)
    let y = Math.Pow(10.0, float (rand.Next(50)) - 25.0)
    x * y

let randomJNumber (rand : Random) =
    JNumber (randomNumber rand)
```

We also want to check that our parser catches invalid numbers, so we'll generate good numbers and
inject an 'x' character in a random spot, or a '0' at the start. This will catch the requirement
that a leading zero not be followed by any more digits prior to the decimal point.

```fsharp
// Generate invalid JSON that will usually fail inside the number parser

let randomBadNumber (rand : Random) =
    let s = (randomNumber rand).ToString()

    match rand.Next(50) with
    | 0 ->
        "0" + s
    | _ ->
        let i = rand.Next(s.Length + 1)
        s.Substring(0, i) + "x" + s.Substring(i)
```        

For strings, we want to be sure to include characters with special escape sequences, as well as
surrogate pair characters. To simplify the surrogate-pair generation we'll use UTF-32 codepoint
values and convert to UTF-16 using .NET text encoding library:

```fsharp
// Generate a random UTF-16-encoded character

let randomChar (rand : Random) = 

    let cp = 
        match rand.Next(10) with
        // surrogate pair required
        | 0 -> rand.Next(0x10000, 0x110000)

        // greater than surrogate range
        | 1 -> rand.Next(0xE000, 0x10000)

        // control character (which would otherwise be rare)
        | 2 -> rand.Next(0x20)

        // less than surrogate range
        | _ -> rand.Next(0xD800)

    Char.ConvertFromUtf32(cp)
```

And then we'll join a random number of characters together:

```fsharp
// Generate a random JSON string

let randomString (rand : Random) =
    [1..rand.Next(20)] |> Seq.map (fun _ -> randomChar rand) |> String.Concat

let randomJString (rand : Random) =
    JString (randomString rand)
```

This generates JSON strings that look something like this:

```
"𢔺\u0010\u0005\u0018\u0017῏\t渲󁚖⫝̸𒤢󒙍\u0012䟳．"
"䞪穩祐ﭾ矂󭈞ᷧ\u000F򩱛蓖􋣺䀖钆祱\u0018뀥鬗묆"
"棗򺙤୅㻦䂸쥊암殿盉Ŭ㴈㩋\u0001䚊猴𭡃촏"
"蝜񔝴簁넘됏뉵ᗽσ긐ﲦ❷"
"픹"
"뚲闠햓\u000F龧󅗟\u000E⹡聻ꥼ崶"
"🏀"
"놷\u0011፧"
"􇤊뼆\b\u001D򣤉呜呯煘\u001Cꄧ褆葺󇒙򎮩ᨿ㕿"
"ᫎ噀﷯"
"ꇚޔ⪍㩀￐򠸴\u0011\u0017얽▥񲠫"
"嚊檚봋萨\u001F"
```

To make invalid strings, we'll randomly insert a character that should be escaped but isn't,
possibly in the middle of a unicode hex value or between a surrogate pair. (It's often hard to
actually see what's wrong with these strings in debug output e.g. in [VS
Code](https://code.visualstudio.com/) because
[IDEs](https://en.wikipedia.org/wiki/Integrated_development_environment) do their own F#-style
escaping of shady stuff.)

```fsharp
// Generate invalid JSON that will fail in the middle string value parsing

let randomBadString (rand : Random) =
    let ch =
        match rand.Next(2) with
        // Control character
        | 0 -> string (char (rand.Next(32))) 

        // Naked surrogate
        | _ -> string (char (rand.Next(0xD800, 0xF000)))

    let s = toStr (randomJString rand)
    let i = rand.Next(1, s.Length) // inside the surrounding quotes
    s.Substring(0, i) + ch + s.Substring(i)
```

For random arrays and objects we can leverage the other random types and package them together, and
then we have a full random JSON value generator. We need some mutally-recursive functions to do
this.

```fsharp
// Generate random nulls, bools, arrays, and objects

let randomJSimple (rand : Random) =
    match rand.Next(3) with
    | 0 -> JNull
    | 1 -> JBool false
    | _ -> JBool true

let rec randomJArray (rand : Random) =
    JArray ([1..rand.Next(7)] |> Seq.map (fun _ -> randomJValue rand) |> Seq.toList)

and randomJObject (rand : Random) =

    let randomJMember (rand : Random) =
        (randomString rand, randomJValue rand)

    JObject ([1..rand.Next(7)] |> Seq.map (fun _ -> randomJMember rand) |> Seq.toList)

and randomJValue (rand : Random) =
    match rand.Next(7) with
    | 0 -> randomJArray rand
    | 1 -> randomJObject rand
    | 2 -> randomJSimple rand
    | 3 -> randomJNumber rand
    | _ -> randomJString rand
```

## Parsing

Now the fun part (for me, anyway). We'll have some core parsing logic that can be used for any text,
and some JSON-specific parsing logic. Before we go into details, here's a little snippet to give the
flavor of what we're going to be ending up with. Here's a parser for JSON numbers. It's essentially
a [DSL](https://en.wikipedia.org/wiki/Domain-specific_language) for a parser. Like FParsec, it's a
recursive descent parser made with parser combinators. These are curried functions that are being
joined together using custom operators.

Note that the spacing in the expression is stretched out to line up with the comments to make it
easier to read here. The code in GitHub doesn't look like that.

```fsharp
// JSON number parser

let jnumber =

    // optional minus sign
    %           (str "-")  

    // and then  a "0"   or else  a non-zero  and then  zero or more  digits
    &.        (str "0"      |.   ('1'--'9'       &.          -.     ('0'--'9')))

    // optional fractional part with decimal point  and  one or more  digits
    &.    %                            (str "."      &.      +.     ('0'--'9'))

    // optional exponent  with E or e    and then  optional   + or -     and then  one or more digits
    &.    %          ((str "E" |. str "e") &.        % (str "+" |. str "-") &.        +.   ('0'--'9'))
    
    // convert the matched text to our JSON model
    |> capture (fun s -> JNumber (Double.Parse(s)))
```

It looks like gibberish at first. It's kinda like a regular expression, but it's actually valid F#
code that compiles and runs and everything. We just need to define some things first to make it
work:

* The `%` operator to make something optional
* The `str` function to look for expected text
* The `&.` and-then operator to join two sub-parsers together end-to-end
* The `|.` or-else operator to choose one sub-parser or another
* The `--` operator to represent a range of allowed characters
* The `-.` zero-or-more unary operator
* The `+.` one-or-more unary operator
* The `capture` function to extract values from matched text

And once we do that we can easily define the parsers for the other JSON data types, or for a wide
variety of general string parsing needs.

Now, I'm not a huge fan of defining a bunch of custom operators. It makes code hard to read. It
makes Haskell code look like Perl. I would normally recommend using named functions whenever
possible. But in this case, when we're definiting a pseudo-Regex like language to define a parser
grammar, it makes sense to do. 

Our parsers will be functions that read text and produce a result. The text will be represented by a
specific location within a string, which I'm calling `Cursor`. By doing this we only have one copy
of the source data in memory and just keep track of "the rest of the text" with an integer index
value.

```fsharp
// Cursor type for keeping track of where we are in the text

type Cursor =
    { text : string
      index : int }

    // Get the next character
    member foo.Head = foo.Get(0)

    // Get the next characters after skipping n
    member foo.Get(n) = 
        if foo.index + n < foo.text.Length then 
            Some foo.text.[foo.index + n] 
        else 
            None

    // Advance the cursor by N characters
    member foo.Skip(n) = { text = foo.text; index = foo.index + n }

    // Check if the specified text is available at the cursor
    member foo.StartsWith(s : string) = 
        foo.index <= foo.text.Length - s.Length && foo.text.Substring(foo.index, s.Length) = s
```

Parsers can fail, so we need to be able to return either a successful or failed result. The failure
case should include the position where the problem was found, and the success case should include
the text we've parsed (in case we want to `capture` it) and the list of captures we've made so far.
This "capture" concept is how we indicate that some of the text is data that we want to keep and
some of it is just delimiters.

```fsharp
type Result<'a> =

// what was expected and where
| Err of string * int   

// matched text, captures, and new cursor
| Ok of string * 'a list * Cursor   
```

We'll start with the simple `str` parser which succeeds if the specified text is found at the
cursor. We need a function that takes a `Cursor` and returns a `Result`.

```fsharp
let str (text : string) (c : Cursor) =
    if c.StartsWith(text) then
        Ok (text, [], c.Skip(text.Length))
    else
        Err (text, c.index)
```

Now if we define the `capture` function, we'll be able to parse `JNull` and `JBool` values which are
simple strings. We have to start somewhere... This is also a parser, just like everything else in
these parser expressions, and it operates on the output of another parser which we must provide as
an argument.

We apply the capturing "mapping" function to the text we've captured so far, and append the
result to the list of captured values in the result.

```fsharp
let capture fn parser (c : Cursor) =
    match parser c with
    | Err (exp, i) -> Err (exp, i)
    | Ok (text, caps, d) -> Ok (text, (fn text) :: caps, d)
```

Now we can define the parsers for the trivial JSON types.

```fsharp
let jnull = 
    str "null" 
    |> capture (fun _ -> JNull)

let jfalse =
    str "false" 
    |> capture (fun _ -> JBool false)

let jtrue =
    str "true" 
    |> capture (fun _ -> JBool true)
```

The next easiest parser is actually going to be the array parser which just involves a few
delimiters around the other parsers. Parsing numbers, strings, and objects is more complicated.

For arrays we'll need the `%` operator for "optional", the `&.` operator for "and then", and the `-.`
unary operator for "zero or more". We'll also go ahead and do the `+.` operator for "one or more"
because it turns out that it's the easiest way to do things.

First the `%` operator. To make another parser optional, we just use the parser and if it returns
and error we just change it into a non-error:

```fsharp
let (~%) parser (c : Cursor) =
    match parser c with
    | Err (exp, i) -> Ok ("", [], c)
    | Ok (text, caps, d) -> Ok (text, caps, d)
```

For the `&.` operator we take two other parsers and we only succeed if they both succeed. The second
parser is provided with the cursor from the result of the first parser. We concatenate the text and
capture results from the two parsers into the combined result.

```fsharp
let (&.) left right (c : Cursor) =
    match left c with
    | Err (exp, i) -> Err (exp, i)
    | Ok (text, caps, d) ->
        match right d with
        | Err (exp, i) -> Err (exp, i)
        | Ok (text2, caps2, e) -> Ok (text + text2, caps @ caps2, e)
```

For the `-.` and `+.` parsers for zero-or-more and one-or-more parsers I was tempted to use some
mutual recursion and some smoke and mirrors, like this:

```fsharp
let rec (~-.) parser = 
    %(+.parser)          // one or more, but optional!

and (~+.) parser =
    parser &. -.parser   // one, and then maybe more!
```

Unfortunately this results in a stack overflow when obtaining the parser, even before trying to use
it. This can be avoided by adding the cursor argument, but the results don't use tail-call
optimization so you end up in a thousand levels of recursion stack just to parse a
thousand-character string literal. I had to get rid of the recursion in one or the other of the
functions, and I sadly ended up with this:

```fsharp
let (~-.) parser c = 
    let b = new StringBuilder()
    let caps = new List<'a list>()
    let mutable cur = c
    while 
        match parser cur with
        | Ok(text, pcaps, c') ->
            b.Append(text) |> ignore
            caps.Add(pcaps)
            cur <- c'
            true
        | _ -> 
            false
        do ()
    Ok(b.ToString(), caps |> Seq.concat |> Seq.toList, cur)

let (~+.) parser =
    parser &. -.parser
```

Let's go ahead and do the `|.` or-else operator which we used for `JNumber`. One thing to note here
is that in case both parsers fail, we report the one that got furthest along because that's more
likely to be where the more useful error is.

```fsharp
let (|.) left right (c : Cursor) =
    match left c with
    | Ok (text, caps, d) -> Ok (text, caps, d)
    | Err (exp, i) ->
        match right c with
        | Ok (text2, caps2, d) -> Ok (text2, caps2, d)
        | Err (exp2, j) -> Err ((if i >= j then exp else exp2), max i j)
```

When we parse an array, the array items will already have been captured by the individual value
parsers, but we'll need to wrap the captures in the `JArray` constructor, so we need a way to
replace the captures with a transformed value. We'll provide a `map` function to do that. It's
similar to the `capture` parser but operates on the captures instead of the text.

```fsharp
let map fn parser (c : Cursor) =
    match parser c with
    | Err (exp, i) -> Err (exp, i)
    | Ok (text, caps, d) -> Ok (text, fn caps, d)
```

Now we can handle JSON values (a few types) and arrays. We'll take care of whitespace handling
around the array delimiters because it's most cleanly handled in the main "value" parser.

```fsharp
let oneOf (chars : string) (c : Cursor) =
    match c.Head with
    | Some ch when chars.Contains(ch) -> Ok(string ch, [], c.Skip(1))
    | _ -> Err ($"one of '{chars}'", c.index)

let whitespace = -.(oneOf " \t\r\n")

let rec valueSelector (c : Cursor) =
    match c.Head with
    | Some 'n' -> jnull c
    | Some 'f' -> jfalse c
    | Some 't' -> jtrue c
    | Some ch when Char.IsDigit(ch) -> jnumber c
    | Some '-' -> jnumber c
    | Some '[' -> jarray c
    (*
    | Some '"' -> jstring c
    | Some '{' -> jobject c
    *)
    | _ -> Err("one of n, f, t, -, \", [, {, or digit", c.index)

and jvalue =
    whitespace &. valueSelector &. whitespace

and jarray =
    ((str "["
        &. %(jvalue &. -.(str "," &. jvalue))
        &. whitespace
        &. str "]")
        |> map (fun list -> [ JArray list ]))
```

I already showed the `JNumber` parser, but it uses one more operator that we haven't defined yet,
the `--` character range operator, e.g. `'0'--'9'` to represent a digit. Here's what that looks
like, and an update of our number parser to take advantage of the `oneOf` that we added.

```fsharp
let (--) lo hi (c : Cursor) =
    match c.Head with
    | Some ch when ch >= lo && ch <= hi -> Ok(string ch, [], c.Skip(1))
    | _ -> Err($"[{lo}-{hi}]", c.index)

let digit<'a> = '0' -- '9'

let jnumber =
    %(str "-")
    &. (str "0" |. ('1' -- '9' &. -.digit))
    &. %(str "." &. +.digit)
    &. %(oneOf "Ee" &. %(oneOf "+-") &. +.digit)
    |> capture (fun s -> JNumber(Double.Parse(s)))
```

Now we just have `JString` and `JObject` left. Object parsing requires string parsing for the member
names, so we need to do strings first.

I-JSON is specific about how it expects [surrogate
pairs](https://www.unicode.org/faq/utf_bom.html#utf16-1) in strings to be handled. Our parsers are
working with F# strings, which are already based on UTF-16 code units (F# and .NET 'char' values),
and strings can already have invalid surrogate combinations. For my own entertainment, I want to
detect and fail on those, but also fail when the surrogate code units are escaped in the JSON text.
If the text is coming from a UTF-8 stream the UTF-8 decoding shouldn't ever produce bad surrogate
combinations, but characters escaped with `\uXXXX` can be invalid. That's what all this extra stuff
is in the string parser.

What I mean by "F# is based on UTF-16" is shown by this example:

```fsharp
let s = "\U0010FFFF"  // one Unicode code point
printfn $"{s.Length}"
```

The string contains a single UTF-32 character, but this prints "2" for the string length. The
character is represented by two chars which form a surrogate pair, which is a UTF-16 concept.

This is what I ended up with, assuming suitable definitions of surrogatePair and nonSurrogate:

```fsharp
let jstring =
    str "\"" &. -.(surrogatePair |. nonSurrogate) &. str "\""
    |> map (fun cs -> [ JString(String.Concat cs) ])
```

Very straightforward. One thing to note is that if we encounter an illegal pattern--one that doesn't
match the surrogatePair or nonSurrogate parsers--it won't show up as the expected error. It will
just end the "more" part of "zero or more", and then it will fail when it expects to find the
terminating double-quote. It'll still do the right thing, and will give the right location for the
error, but it will give a misleading indicator of what it was expecting to find. This is a general
problem, and is fixable, but is outside the scope of this article. Check the Github repo to see if
I've fixed it yet.

To parse a non-surrogate character:

```fsharp
let nonSurrogate =

    // non-escaped, non-control, and not quote (0x22) or backslash (0x5C)
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
```

The `nonSurrogateCode` parser matches a hex code that excludes the surrogate range, and the
`codeUnit` capture function converts this code into a character.

```fsharp
let hexChar<'a> = oneOf "0123456789AaBbCcDdEeFf"

let nonSurrogateCode =
    ((oneOf "0123456789AaBbCcEeFf" &. hexChar) // 00 to CF, or E0 to FF
        |. (oneOf "Dd" &. oneOf "01234567")) // D0 to D7
    &. hexChar
    &. hexChar

let codeUnit (s: string) = char (Convert.ToInt32(s, 16))
```

Handling surrogate pairs is all about the hex codes:

```fsharp
let hiSurrogateCode =
    oneOf "Dd" &. oneOf "89AaBb" &. hexChar &. hexChar

let loSurrogateCode =
    oneOf "Dd" &. oneOf "CcDdEeFf" &. hexChar &. hexChar

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
```

Now that we have string parsing, we can finish off with the `JObject` parsing. The `and` means this
is part of the mutually-recursive `jvalue` and `jarray`, so it has to go right after or in between
those.

There's an ugly wrinkle in here. In `jobject` I've repeated the definition of the `whitespace`
parser to handle the whitespace after the opening brace. I could have fixed this by changing the
structure of the object parser, but the fix seemed more complicated than the workaround. The problem
happens because type inference results in `whitespace` having concrete type `Cursor -> Result<Json>`
whereas in the object parser we need a `Cursor -> Result<string * Json>` because the captured values
are key-value pairs. Something to fix another day. I gots deadlines.

```fsharp
and jobject =

    let field =
        whitespace &. jstring &. whitespace &. str ":" &. jvalue &. whitespace
        |> map (fun [ JString name; value ] -> [ name, value ])

    (str "{"
        &. -.(oneOf " \t\r\n")
        &. %(field &. -.(str "," &. field))
        &. str "}")
    |> map (fun list -> [ JObject list ])
```

Now we go back and un-comment-out the jstring and jobject options in `valueSelector`, and then
finish off with a helper function that wraps the input string in a `Cursor` and ensures that we've
consumed the entire string:

```fsharp
let jparser s =

    let result = jvalue { text = s; index = 0 }

    match result with
    | Ok (_, _, c) when c.index <> s.Length -> Err("end of text", c.index)
    | _ -> result
```

## GeoJSON

Coming soon.
