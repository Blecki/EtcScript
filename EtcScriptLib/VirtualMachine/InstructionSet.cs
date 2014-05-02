using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
    public enum InstructionSet
    {
        MOVE,           // SOURCE       DESTINATION     UNUSED

		ALLOC_RSO,		// SIZE			DESTINATION
		LOAD_RSO_M,		// RSO			MEMBER			DESTINATION
		STORE_RSO_M,	// VALUE		RSO				MEMBER

		LOAD_STATIC,	// OFFSET		DESTINATION
		STORE_STATIC,	// VALUE		OFFSET

		IS_ANCESTOR_OF, // DESCENDANT	ANCESTOR		DESTINATION

		MARK_STACK,
		RESTORE_STACK,
        RETURN,       // SOURCE       UNUSED          UNUSED      --Moves execution to the point in SOURCE, without advancement.
        CLEANUP,        // SOURCE       UNUSED          UNUSED      --Remove SOURCE items from top of stack.
		JUMP,			// SOURCE									--Jump to an absolute address in the current instruction stream

        EMPTY_LIST,     // DESTINATION  UNUSED          UNUSED      --Create an empty list and store in DESTINATION.
        APPEND,         // VALUE        LIST            DESTINATION --Append VALUE to LIST, store in DESTINATION.

        //INVOKE,
		STACK_INVOKE,
		COMPAT_INVOKE,
		CALL,
        LAMBDA,

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
