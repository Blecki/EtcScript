using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ninbot.VirtualMachine
{
    public enum InstructionSet
    {
        YIELD = 0,      //Yield execution back to the system

        MOVE,           // SOURCE       DESTINATION     UNUSED
        LOOKUP,         // NAME         DESTINATION     UNUSED
        LOOKUP_MEMBER,  // NAME         OBJECT          DESTINATION
        SET_MEMBER,     // VALUE        NAME            OBJECT
        RECORD,         // DESTINATION  UNUSED          UNUSED      --Create an empty record and store in DESTINATION.

        MARK,           // DESTINATION  UNUSED          UNUSED      --Places the current execution point in DESTINATION.
		MARK_STACK,
		RESTORE_STACK,
        BREAK,          // SOURCE       UNUSED          UNUSED      --Moves execution to the point in SOURCE, skipping 1 instruction.
        BRANCH,         // CODE         DESTINATION     UNUSED      --MARK; then move execution into embedded code CODE.
        CONTINUE,       // SOURCE       UNUSED          UNUSED      --Moves execution to the point in SOURCE, without advancement.
        CLEANUP,        // SOURCE       UNUSED          UNUSED      --Remove SOURCE items from top of stack.
        SWAP_TOP,       // UNUSED       UNUSED          UNUSED      --Swap the two top object on stack.

        EMPTY_LIST,     // DESTINATION  UNUSED          UNUSED      --Create an empty list and store in DESTINATION.
        APPEND_RANGE,   // LIST-A       LIST-B          DESTINATION --Append A to B, store in DESTINATION.
        APPEND,         // VALUE        LIST            DESTINATION --Append VALUE to LIST, store in DESTINATION.
        LENGTH,         // LIST         DESTINATION     UNUSED      --Place the length of LIST in DESTINATION.
        INDEX,          // INDEX        LIST            DESTINATION --Place LIST[INDEX] in DESTINATION.
        PREPEND,
        PREPEND_RANGE,

        INVOKE,
        LAMBDA,
		SET_FRAME,		// SOURCE									Replace the current frame with SOURCE.

        SET_VARIABLE,

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
