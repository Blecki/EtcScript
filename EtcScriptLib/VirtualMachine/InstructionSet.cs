using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
    public enum InstructionSet
    {
        YIELD = 0,      //Yield execution back to the system

        MOVE,           // SOURCE       DESTINATION     UNUSED
        LOOKUP,         // NAME         DESTINATION     UNUSED
		DYN_LOOKUP,		// NAME			DESTINATION		UNUSED
        LOOKUP_MEMBER,  // NAME         OBJECT          DESTINATION	--Look for a member named 'name' on object. This is a hard
																	//query: If there is no matching member, it is an error.
		DYN_LOOKUP_MEMBER,	// NAME			OBJECT			DESTINATION	--Look for a member named 'name' on object. If found, R is 
																	//set to true. If not, R is set to false.
        SET_MEMBER,     // VALUE        NAME            OBJECT
		DYN_SET_MEMBER, // VALUE		NAME			OBJECT		--Like SET_MEMBER, except R indicates success. Does not produce errors.
        RECORD,         // DESTINATION  UNUSED          UNUSED      --Create an empty record and store in DESTINATION.

        MARK,           // DESTINATION  UNUSED          UNUSED      --Places the current execution point in DESTINATION.
		MARK_STACK,
		RESTORE_STACK,
        BREAK,          // SOURCE       UNUSED          UNUSED      --Moves execution to the point in SOURCE, skipping 1 instruction.
        BRANCH,         // CODE         DESTINATION     UNUSED      --MARK; then move execution into embedded code CODE.
        CONTINUE,       // SOURCE       UNUSED          UNUSED      --Moves execution to the point in SOURCE, without advancement.
        CLEANUP,        // SOURCE       UNUSED          UNUSED      --Remove SOURCE items from top of stack.
        SWAP_TOP,       // UNUSED       UNUSED          UNUSED      --Swap the two top object on stack.
		JUMP,			// SOURCE									--Jump to an absolute address in the current instruction stream
		JUMP_RELATIVE,	// SOURCE									--Add source to the current instruction pointer
		JUMP_MARK,		// SOURCE		DESTINATION					--MARK to DESTINATION, and then JUMP to source

        EMPTY_LIST,     // DESTINATION  UNUSED          UNUSED      --Create an empty list and store in DESTINATION.
        APPEND_RANGE,   // LIST-A       LIST-B          DESTINATION --Append A to B, store in DESTINATION.
        APPEND,         // VALUE        LIST            DESTINATION --Append VALUE to LIST, store in DESTINATION.
        LENGTH,         // LIST         DESTINATION     UNUSED      --Place the length of LIST in DESTINATION.
        INDEX,          // INDEX        LIST            DESTINATION --Place LIST[INDEX] in DESTINATION.
        PREPEND,
        PREPEND_RANGE,
		REPLACE_FRONT,	// VALUE		LIST			DESTINATION

        INVOKE,
		STACK_INVOKE,
		CALL,
        LAMBDA,
		SET_FRAME,		// SOURCE									Replace the current frame with SOURCE.

        SET_VARIABLE,
		LOAD_PARAMETER,		// SOURCE-OFFSET		DESTINATION
		STORE_PARAMETER,	// SOURCE		DESTINATION-OFFSET

        DECREMENT,
        INCREMENT,
        LESS,
		GREATER,
		LESS_EQUAL,
        GREATER_EQUAL,
        IF_TRUE,
        IF_FALSE,
        SKIP,
        EQUAL,
		NOT_EQUAL,

        THROW,
        CATCH,

        ADD,
        SUBTRACT,
        MULTIPLY,
        DIVIDE,
		AND,
		OR,
		MODULUS,

		LOR,
		LAND,

        DEBUG,
    }
}
