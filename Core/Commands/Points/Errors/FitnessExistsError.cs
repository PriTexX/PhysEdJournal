namespace Core.Commands;

public sealed class FitnessExistsError() : Exception("Fitness already exists for student") { }
