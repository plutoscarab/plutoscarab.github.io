namespace PlutoScarab

open System.Collections.Generic
open System.Text

module Parser =

    // Cursor type for keeping track of where we are in the text

    type Cursor =
        { text : string
          index : int }

        // Get the next character
        member foo.Head = foo.Get(0)

        // Get the next N characters
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


    type Result<'a> =
    | Ok of string * 'a list * Cursor
    | Err of string * int   
    | Fatal of string * int

    let str (text : string) (c : Cursor) =
        if c.StartsWith(text) then
            Ok (text, [], c.Skip(text.Length))
        else
            Err (text, c.index)


    let capture fn parser (c : Cursor) =
        let result = parser c
        match result with
        | Ok (text, caps, d) -> Ok (text, (fn text) :: caps, d)
        | _ -> result


    let map fn parser (c : Cursor) =
        match parser c with
        | Ok (text, caps, d) -> Ok (text, fn caps, d)
        | Err (exp, i) -> Err (exp, i)
        | Fatal (exp, i) -> Fatal (exp, i)


    let (~%) parser (c : Cursor) =
        let result = parser c
        match result with
        | Err (_, _) -> Ok ("", [], c)
        | _ -> result

    
    let (&.) left right (c : Cursor) =
        let result1 = left c
        match result1 with
        | Ok (text, caps, d) ->
            let result2 = right d
            match result2 with
            | Ok (text2, caps2, e) -> Ok (text + text2, caps @ caps2, e)
            | _ -> result2
        | _ -> result1


    let (|.) left right (c : Cursor) =
        let result1 = left c
        match result1 with
        | Err (exp, i) ->
            let result2 = right c
            match result2 with
            | Err (exp2, j) -> Err ((if i >= j then exp else exp2), max i j)
            | _ -> result2
        | _ -> result1


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


    let oneOf (chars : string) (c : Cursor) =
        match c.Head with
        | Some ch when chars.Contains(ch) -> Ok(string ch, [], c.Skip(1))
        | _ -> Err ($"one of '{chars}'", c.index)


    let anyBut (chars : string) (c : Cursor) =
        match c.Head with
        | Some ch when not (chars.Contains(ch)) -> Ok(string ch, [], c.Skip(1))
        | _ -> Err($"not one of '{chars}'", c.index)

    let (--) lo hi (c : Cursor) =
        match c.Head with
        | Some ch when ch >= lo && ch <= hi -> Ok(string ch, [], c.Skip(1))
        | _ -> Err($"[{lo}-{hi}]", c.index)

    let anyFatal parser c =
        let result = parser c
        match result with
        | Err(exp, i) ->
            if i > c.index then
                Fatal (exp, i)
            else
                result
        | _ -> result

    let fatalIf parser c =
        let result = parser c
        match result with
        | Ok(text, _, _) -> Fatal($"not {text}", c.index)
        | _ -> result
        
