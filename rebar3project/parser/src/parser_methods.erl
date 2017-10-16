%%%-------------------------------------------------------------------
%%% @author Erik
%%% @copyright (C) 2017, <COMPANY>
%%% @doc
%%%
%%% @end
%%% Created : 15. okt 2017 23:44
%%%-------------------------------------------------------------------
-module(parser_methods).
-author("Erik").

%% API

-export([encode/1, parse_to_map/1, get_SD/0, get_DD/0, get_CD/0, get_processes/1, get_classes/1,
  get_relationships/1, get_type/1, get_mapping/1, get_diagram/1, get_diagram_contents/1]).

%% Returns the JSON as an Erlang map without the meta data
parse_to_map(X) -> Z = remove_meta(decode_map(X)), io:format("The ~p map has the following keys: ~p~n~n",
  [get_type(Z), maps:keys(Z)]), Z.

%% Decodes the JSON file into an Erlang map
decode_map(X) ->
  case jsx:is_json(X) of
    true  -> jsx:decode(X, [return_maps]);
    false -> 'not a valid JSON'
  end.

%% Removes potential meta data from decoded JSON, if no meta present, just returns X
remove_meta(X) -> maps:remove(<<"meta">>, X).

%% Returns the diagram type
get_type(X) -> maps:get(<<"type">>, X).

%% Returns the diagram
get_diagram(X) -> case get_type(X) of
                    <<"sequence_diagram">> -> maps:get(<<"diagram">>, X);
                    _Else -> 'Error, not a sequence diagram'
                  end.

%% Returns the diagram contents in a list
get_diagram_contents(X) -> case get_type(X) of
                             <<"sequence_diagram">> -> maps:get(<<"content">>, parser:get_diagram(X));
                             _Else -> 'Error, not a sequence diagram'
                           end.

%% Returns the processes in a list
get_processes(X) -> case get_type(X) of
                      <<"sequence_diagram">> -> maps:get(<<"processes">>, X);
                      _Else -> 'Error, not a sequence diagram'
                    end.

%% Returns the diagram type
get_classes(X) -> case get_type(X) of
                    <<"class_diagram">> -> maps:get(<<"classes">>, X);
                    _Else -> 'Error, not a class diagram'
                  end.

%% Returns the relationships in a list
get_relationships(X) -> case get_type(X) of
                          <<"class_diagram">> -> maps:get(<<"relationships">>, X);
                          _Else -> 'Error, not a class diagram'
                        end.

%% Returns the diagram type
get_mapping(X) -> case get_type(X) of
                    <<"deployment_diagram">> -> maps:get(<<"mapping">>, X);
                    _Else -> 'Error, not a deployment diagram'
                  end.

%% Converts Erlang binary map into JSON string
encode(X) ->
  io:format("Encoding to JSON string ~p~n", ['...']), jsx:encode(X).

%% Returns a JSON sequence diagram
get_SD() ->
  {ok, File} = file:read_file("SD.json"), File.

%% Returns a JSON class diagram
get_CD() ->
  {ok, File} = file:read_file("CD.json"), parse_to_map(File).

%% Returns a JSON deployment diagram
get_DD() ->
  {ok, File} = file:read_file("DD.json"), parse_to_map(File).