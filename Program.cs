using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blender;

public class Configuration
{
    public int NumTanks { get; set; }
    // Add any other necessary properties for the configuration
}

public class State
{
    public int NumTanks => Configuration.NumTanks;
    public Configuration Configuration { get; }
    public IReadOnlyList<Mix> Contents { get; }
    public IEnumerable<Mix> Mixes => Contents.Where(m => m != null)!;
    public int Depth { get; }
    public int StateId { get; }
    public double Volume { get; }
    public int UsedTanks { get; }
    public double TotalWine => Contents.Sum(m => m?.Sum ?? 0);
    public IReadOnlyList<Transfer> Transfers => ComputeTransfers();

    public State(Configuration configuration, IReadOnlyList<Mix> contents, int depth = 0)
    {
        Configuration = configuration;
        Contents = contents;
        Depth = depth;
        StateId = GenerateStateId();
        Volume = CalculateVolume();
        UsedTanks = CountUsedTanks();
    }

    public static State Create(Configuration configuration)
    {
        var contents = Enumerable.Repeat<Mix>(null, configuration.NumTanks).ToList();
        return new State(configuration, contents);
    }

    public bool IsOccupied(int i)
    {
        return Contents[i] != null;
    }

    public Mix this[int i]
    {
        get { return Contents[i]; }
        set { Contents[i] = value; }
    }

    public double GetTankSize(int i)
    {
        // Assuming tank sizes are the same for all tanks in the configuration
        return 1.0 / Configuration.NumTanks;
    }

    public double TargetDistance(Mix mix)
    {
        return Math.Abs(mix.Sum - 1);
    }

    public IEnumerable<TankList> OccupiedTankLists()
    {
        for (int i = 0; i < NumTanks; i++)
        {
            if (IsOccupied(i))
            {
                yield return new TankList(i);
            }
        }
    }

    public IEnumerable<TankList> UnoccupiedTankLists(int volume)
    {
        if (volume == 0)
        {
            yield return new TankList();
        }
        else
        {
            for (int i = 0; i < NumTanks; i++)
            {
                if (!IsOccupied(i))
                {
                    foreach (var tankList in UnoccupiedTankLists(volume - 1))
                    {
                        yield return new TankList(i, tankList);
                    }
                }
            }
        }
    }

    private IReadOnlyList<Transfer> ComputeTransfers()
    {
        var transfers = new List<Transfer>();

        foreach (var from in OccupiedTankLists())
        {
            foreach (var to in UnoccupiedTankLists(from.Volume))
            {
                if (from.IsAdjacent(to))
                {
                    transfers.Add(new Transfer(from, to));
                }
            }
        }

        return transfers;
    }

    public Mix GetMix(TankList tanks)
    {
        double sum = tanks.Tanks.Sum(i => Contents[i].Sum);
        return new Mix { Sum = sum };
    }

    public Mix GetMix(Transfer transfer)
    {
        double sum = transfer.From.Tanks.Sum(i => Contents[i].Sum);
        return new Mix { Sum = sum };
    }

    public bool IsOccupied(TankList tanks)
    {
        return tanks.Tanks.All(IsOccupied);
    }

    public bool IsUnoccupied(TankList tanks)
    {
        return tanks.Tanks.All(i => !IsOccupied(i));
    }

    public bool IsTransferValid(Transfer transfer)
    {
        return IsOccupied(transfer.From) && IsUnoccupied(transfer.To);
    }

    public State Apply(Transfer transfer)
    {
        var newContents = new List<Mix>(Contents);

        foreach (var tankIndex in transfer.From.Tanks)
        {
            newContents[tankIndex] = null;
        }

        foreach (var tankIndex in transfer.To.Tanks)
        {
            newContents[tankIndex] = GetMix(transfer);
        }

        return new State(Configuration, newContents, Depth + 1);
    }

    public StringBuilder BuildString(StringBuilder sb = null, bool contents = true)
    {
        sb ??= new StringBuilder();
        sb.AppendLine($"State ID: {StateId}");
        sb.AppendLine($"Depth: {Depth}");
        sb.AppendLine($"Volume: {Volume}");
        sb.AppendLine($"Used Tanks: {UsedTanks}");
        sb.AppendLine($"Total Wine: {TotalWine}");
        
        if (contents)
        {
            sb.AppendLine("Tank Contents:");
            for (int i = 0; i < NumTanks; i++)
            {
                var mix = Contents[i];
                sb.AppendLine($"Tank {i + 1}: {mix?.Sum ?? 0}");
            }
        }

        return sb;
    }

    public IEnumerable<State> GetNextStates()
    {
        foreach (var transfer in Transfers)
        {
            if (IsTransferValid(transfer))
            {
                yield return Apply(transfer);
            }
        }
    }

    public void CheckTotalWineIsValid()
    {
        double totalWineInConfig = Configuration.NumTanks * (1.0 / Configuration.NumTanks);
        if (Math.Abs(TotalWine - totalWineInConfig) > double.Epsilon)
        {
            throw new InvalidOperationException("Total wine in the state does not match the initial wine amount specified in the configuration.");
        }
    }

    public void CheckThatTankAmountsAreValid()
    {
        for (int i = 0; i < NumTanks; i++)
        {
            double tankSize = GetTankSize(i);
            double tankAmount = Contents[i]?.Sum ?? 0;
            if (Math.Abs(tankAmount - tankSize) > double.Epsilon)
            {
                throw new InvalidOperationException($"The amount of wine in tank {i} does not match its size specified in the configuration.");
            }
        }
    }

    public Mix BestMix()
    {
        Mix bestMix = null;
        double lowestTargetDistance = double.PositiveInfinity;

        foreach (var mix in Mixes)
        {
            double targetDistance = TargetDistance(mix);
            if (targetDistance < lowestTargetDistance)
            {
                bestMix = mix;
                lowestTargetDistance = targetDistance;
            }
        }

        return bestMix;
    }

    private int GenerateStateId()
    {
        // Replace with your logic to generate a unique state id
        return 0;
    }

    private double CalculateVolume()
    {
        // Assuming all tanks have the same size in the configuration
        return Configuration.NumTanks * GetTankSize(0);
    }

    private int CountUsedTanks()
    {
        return Contents.Count(m => m != null);
    }
}
public class Program
{
    public static void Main()
    {
        // Create a configuration
        Configuration configuration = new Configuration
        {
            NumTanks = 4
            // Set other properties for the configuration as needed
        };

        // Create a new state based on the configuration
        State initialState = State.Create(configuration);

        // Access properties of the state
        int numTanks = initialState.NumTanks;
        Configuration stateConfiguration = initialState.Configuration;
        IReadOnlyList<Mix> stateContents = initialState.Contents;
        // Access other properties as needed

        // Access the mix in a tank
        Mix mixInTank = initialState[0];

        // Apply a transfer to the state
        Transfer transfer = new Transfer(new TankList(0), new TankList(1));
        State nextState = initialState.Apply(transfer);

        // Get the next possible states
        IEnumerable<State> nextStates = initialState.GetNextStates();

        // Build a string representation of the state
        StringBuilder stateStringBuilder = initialState.BuildString();

        // Perform other operations on the state as needed

        // Display the results
        Console.WriteLine($"Number of Tanks: {numTanks}");
        Console.WriteLine($"Configuration: {stateConfiguration}");
        // Display other properties and results as needed
    }
}