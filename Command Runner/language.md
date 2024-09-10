everything's a command



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
		- -    sign negate
	- __binary_op_identifier__
		- >>   arithmetic right shift
		- <<   arithmetic/logicals left shift
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
		- %    modulus
		- **   exponentiation
		
	

# _command_
	- _literal_command_
		- _numeric_value_return_command_ = ^-?[0-9]+(__integral_numeral_identifier__|((\.[0-9]+)?__rational_numeral_identifier__))$
		- _string_value_return_command_ = ^""([^\\""]|__escapes__)*""$	
	- _variable_unwrapped_value_return_command_ = __variable_command_identifier__
	- _variable_assign_command_ = __variable_command_identifier__ = __arg__
	- _variable_assign_command_value_return_command_ = __variable_command_identifier__ >= __arg__
	- _chain_command_ = __arg__._command_
	- _chain_command_value_return_command_ = __arg__>._command_
	- _icommand_execute_command_ = __arg__\(__arg__, ...\)
		- first arg must execute to an ICommand
	- _evaluate_group_command_ = \(_command_\)
	- _command_value_return_command_ = >_command_
		- returns a command that returns the command that's the operator's argument
	- _unary_op_command_ = __unary_op_identifier__ __arg__
	- _unary_op_command_value_return_command_ = >__unary_op_identifier__ __arg__
	- _binary_op_command_ = __arg__ __binary_op_identifier__ __arg__
	- _binary_op_command_value_return_command_ = __arg__ >__binary_op_identifier__ __arg__
