everything's a command
everything's left associative


# __identifier__
	- __command_identifier__ = ^[_a-zA-Z][_0-9a-zA-Z]*$
		- __variable_command_identifier__ = \$__command_identifier__
		- __meta_command_identifier__ = ~__command_identifier__
		- __innate_command_identifier__ = __command_identifier__
	- __integral_numeral_identifier__ = [zbsil]
			- z int1 (C# bool)
			- b int8
			- s int16
			- i int32
			- l int64
	- __rational_numeral_identifier__ = [hfdm]
			- h half
			- f single
			- d double
			- m decimal (C# decimal)
	- __escapes__ = (\\\\)|(\\"")
		- \\ \
		- \" "
	- __arg__ = _command_|\?
		- if _command_ executes the command
		- ? adds a parameter to resulting command.
	- __unary_op_identifier__
		- ~    complement
		- ++   increment
		- --   decrement
			- maybe make post for unabiguity and familiarity
		- -    _sign_negate_
	- __binary_op_identifier__
		- >>   arithmetic right shift
		- <<   arithmetic/logical left shift
		- >>>  logical right shift
		- (<)  circular left shirt
		- (>)  circular right shirt
		- ^    exclusive disjunction
		- |    inclusive disjunction
		- &    conjunction
		- +    addition
		- -    subtraction
		- /    division
		- *    multiplication
		- %    modulo
		- **   exponentiation
		
	

# _command_
	- _literal_command_
		- _numeric_literal_command_ = ^-?[0-9]+(__integral_numeral_identifier__|((\.[0-9]+)?__rational_numeral_identifier__))$
			- returns numeric literal on execution
			- built in negative here is technically indistiguishable from a unary negate
		- _string_literal_command_ = ^""([^\\""]|__escapes__)*""$	
			- returns string literal on execution
	- _parenthesis_enclosed_command_ = \(__arg__\)
		- returns argument result
	- _unexecuted_command_ = >_command_
		- returns a command that returns the command that's the operator's argument
	
	- _variable_unwrap_command_ = __variable_command_identifier__
		- returns contained value
	- _variable_assign_command_ = __variable_command_identifier__ = __arg__

	- _chain_command_ = __arg__._command_
	- _command_execute_command_ = __command_identifier__\(__arg__...\)
		- could make work on any _command_ instead, but that case becomes ambiguous around _variable_unwrap_command_

	- _unary_op_command_ = __unary_op_identifier__ __arg__
	- _binary_op_command_ = __arg__ __binary_op_identifier__ __arg__



# character evaluation tree
[enter]
	-> [command]
		-?> [end]
		-> [__arg__]

	
[command]
	-?> "
		-?> _string_literal_command_
	-?> -?[0-9]
		-?> _numeric_literal_command_



	-?> (
		-?> __arg__
			-?> )
				-> [compose](_parenthesis_enclosed_command_)
	-?> >
		-?> _command_
			-?> _unexecuted_command_



	-?> $
		-?> __variable_command_identifier__
			-?> =
				-?> __arg__
					-> [compose](_variable_assign_command_)
			-?> (
				-?> unwraps to ICommand
					-> [compose](_command_execute_command_)
			-?> _variable_unwrap_command_
	-?> ~
		-?> __meta_command_identifier__
			-> [compose](_command_execute_command_)
	-?> __innate_command_identifier__
		-> [compose](_command_execute_command_)



	-?> ?
		-> [__arg__]
		- possibly have enter not err after this, instead return some kind of _parameterize_command_ that simply return it's argument on execute. will complicate parameterize everywhere else.
	-?> __unary_op_identifier__
		-?> __arg__
			-> [compose](_unary_op_command_)

[__arg__]
	-?> _chain_command_
	-?> _binary_op_command_


[compose](_command_, __arg__...)
	- returned ICommand has param count equal to number of null Arguments
	- Non null Arguments pass directly, parameterized map in order of occurence to returned's parameters.