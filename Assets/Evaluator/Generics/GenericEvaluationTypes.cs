using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface Feedable<In, Out>
{
    public abstract In Input { get; }
    public abstract Out Output { get; set; }
}


public class InputOutput<In, Out> : Feedable<In, Out>
{
    public In Input { get { return input; } protected set { input = value; } }
    public Out Output { get { return output; } set { output = value; } }

    private In input;
    private Out output;
}
