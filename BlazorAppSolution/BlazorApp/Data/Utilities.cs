﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOPsReview
{
    public static class Utilities
    {
        //static classes are NOT instantiated by the outside user (developer/code)
        //static class items are referenced using:  classname.xxxx
        //methods within this class have the keyword static in their method header
        //static classes are shared between all outside users at the same time
        //DO NOT consider saving data within a static class BECAUSE you cannot be
        //  certain it will be there when you use the class again
        //consider placing GENERIC re-usable methods within a static class

        //sample of a generic method: check a numeric is a zero or positive value
        public static bool IsZeroOrPositive(double value)
        {
            //this is an example of unstructured code
            //multiple exits from the if structure AND the method
            //this would violate the class standard of structured code
            //if (value < 0)
            //    return false;
            //else
            //    return true;

            //a structure method (apply to loops, etc) will have a single entry point
            //  and a single exit point
            //in this course you WILL AVOID where possible multiple returns from a method
            //in this course you WILL AVOID using a break or continue or exit to exit a loop structure
            //  or if structure or method
            //this does not apply to the switch break; which is part of its design
            bool valid = true; //assume that the value entering the method is correct
            if (value < 0.0)
                valid = false; //the has determind that the value is incorrect
            return valid;
        }

        //overload the IsZeroOrPositive so that it uses integers or decimals
        //overload methods have different signatures
        //the C# accept definition of a method signature is :   methodname(list of parameters)
        public static bool IsZeroOrPositive(int value)
        {
            bool valid = true; //assume that the value entering the method is correct
            if (value < 0)
                valid = false; //the has determind that the value is incorrect
            return valid;
        }
        public static bool IsZeroOrPositive(decimal value)
        {
            bool valid = true; //assume that the value entering the method is correct
            if (value < 0.0m)
                valid = false; //the has determind that the value is incorrect
            return valid;
        }
    }
}
