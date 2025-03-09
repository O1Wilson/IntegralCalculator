using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Linq.Dynamic.Core;

public enum IntegrationMethod {
    Left,
    Right,
    Midpoint,
    Trapezoidal,
    Simpson
}

class Program {
    static void Main(string[] args) {
        Console.WriteLine("Enter an equation in terms of x");
        string equation = Console.ReadLine() ?? "";

        Console.WriteLine("How many partitions would you like?");
        if (!int.TryParse(Console.ReadLine(), out int partitions) || partitions <= 0) {
            Console.WriteLine("Invalid partitions value. Must be a positive integer.");
            return;
        }

        Console.WriteLine("Enter domain bounds [x0,x1]: ");
        string input = Console.ReadLine() ?? "";
        string[] parts = input.Trim('[', ']').Split(',');
        if (!(parts.Length == 2 &&
              double.TryParse(parts[0], out double x0) &&
              double.TryParse(parts[1], out double x1))) {
            Console.WriteLine("Invalid input format. Please enter values as [x0,x1].");
            return;
        }

        if (x0 > x1) {
            double temp = x0;
            x0 = x1;
            x1 = temp;
        }

        Console.WriteLine("Select an integration method:");
        Console.WriteLine("1. Left Endpoint");
        Console.WriteLine("2. Right Endpoint");
        Console.WriteLine("3. Midpoint");
        Console.WriteLine("4. Trapezoidal");
        Console.WriteLine("5. Simpson's Rule");
        if (!int.TryParse(Console.ReadLine(), out int methodChoice) || methodChoice < 1 || methodChoice > 5) {
            Console.WriteLine("Invalid method selection.");
            return;
        }
        IntegrationMethod method = (IntegrationMethod)(methodChoice - 1);

        Func<double, double> func;
        try {
            func = CompileEquation(equation);
        }
        catch (Exception ex)  {
            Console.WriteLine("Error compiling the equation: " + ex.Message);
            return;
        }

        double area = 0;
        try {
            switch (method) {
                case IntegrationMethod.Left:
                    area = LeftEndpointIntegration(func, x0, x1, partitions);
                    break;
                case IntegrationMethod.Right:
                    area = RightEndpointIntegration(func, x0, x1, partitions);
                    break;
                case IntegrationMethod.Midpoint:
                    area = MidpointIntegration(func, x0, x1, partitions);
                    break;
                case IntegrationMethod.Trapezoidal:
                    area = TrapezoidalIntegration(func, x0, x1, partitions);
                    break;
                case IntegrationMethod.Simpson:
                    if (partitions % 2 != 0) {
                        Console.WriteLine("Simpson's rule requires an even number of partitions. Incrementing partitions by 1.");
                        partitions++;
                    }
                    area = SimpsonsRuleIntegration(func, x0, x1, partitions);
                    break;
                default:
                    Console.WriteLine("Unsupported integration method.");
                    return;
            }
        }
        catch (Exception ex) {
            Console.WriteLine("Error during integration: " + ex.Message);
            return;
        }

        Console.WriteLine($"The approximate area under the curve using {method} method is: {area}");
    }

    static Func<double, double> CompileEquation(string equation) {
        string modifiedEquation = ConvertExponents(equation);
        var param = Expression.Parameter(typeof(double), "x");
        var lambda = DynamicExpressionParser.ParseLambda(new[] { param }, typeof(double), modifiedEquation);
        return (Func<double, double>)lambda.Compile();
    }

    static string ConvertExponents(string equation) {
        Regex regex = new Regex(@"(\([^()]+\)|\d+(\.\d+)?|x)\^(\d+(\.\d+)?)");
        while (regex.IsMatch(equation)) {
            equation = regex.Replace(equation, match => {
                string baseNum = match.Groups[1].Value;
                string exponent = match.Groups[3].Value;
                return $"Math.Pow({baseNum}, {exponent})";
            });
        } return equation;
    }

    static double LeftEndpointIntegration(Func<double, double> func, double x0, double x1, int partitions) {
        double deltaX = (x1 - x0) / partitions;
        double sum = 0;
        for (int i = 0; i < partitions; i++) {
            double x = x0 + i * deltaX;
            sum += func(x);
        } return sum * deltaX;
    }

    static double RightEndpointIntegration(Func<double, double> func, double x0, double x1, int partitions) {
        double deltaX = (x1 - x0) / partitions;
        double sum = 0;
        for (int i = 1; i <= partitions; i++) {
            double x = x0 + i * deltaX;
            sum += func(x);
        } return sum * deltaX;
    }

    static double MidpointIntegration(Func<double, double> func, double x0, double x1, int partitions) {
        double deltaX = (x1 - x0) / partitions;
        double sum = 0;
        for (int i = 0; i < partitions; i++) {
            double x = x0 + (i + 0.5) * deltaX;
            sum += func(x);
        } return sum * deltaX;
    }

    static double TrapezoidalIntegration(Func<double, double> func, double x0, double x1, int partitions) {
        double deltaX = (x1 - x0) / partitions;
        double sum = 0;
        for (int i = 0; i < partitions; i++) {
            double xLeft = x0 + i * deltaX;
            double xRight = x0 + (i + 1) * deltaX;
            sum += (func(xLeft) + func(xRight)) / 2;
        } return sum * deltaX;
    }

    static double SimpsonsRuleIntegration(Func<double, double> func, double x0, double x1, int partitions) {
        if (partitions % 2 != 0)
            throw new ArgumentException("Simpson's rule requires an even number of partitions.");
        double deltaX = (x1 - x0) / partitions;
        double sum = func(x0) + func(x1);
        for (int i = 1; i < partitions; i++) {
            double x = x0 + i * deltaX;
            sum += (i % 2 == 0 ? 2 : 4) * func(x);
        } return sum * deltaX / 3;
    }
}