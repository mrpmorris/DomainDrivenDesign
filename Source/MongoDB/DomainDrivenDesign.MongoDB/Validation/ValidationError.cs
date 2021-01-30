using System;

namespace DomainDrivenDesign.MongoDB.Validation
{
	public struct ValidationError : IEquatable<ValidationError>
	{
		public string? ErrorCode { get; set; }
		public string ErrorMessage { get; set; }
		public string MemberPath { get; set; }

		public ValidationError(
			string memberPath,
			string errorMessage,
			string? errorCode = null)
		{
			ErrorCode = errorCode;
			ErrorMessage = errorMessage;
			MemberPath = memberPath;
		}

		public override bool Equals(object obj)
		{
			return obj is ValidationError error && Equals(error);
		}

		public bool Equals(ValidationError other)
		{
			return
				ErrorCode == other.ErrorCode
				&& MemberPath == other.MemberPath
				&& ErrorMessage == other.ErrorMessage;
		}

		public override int GetHashCode() =>
			$"{ErrorCode}-{MemberPath}-{ErrorMessage}".GetHashCode();

		public static bool operator ==(ValidationError left, ValidationError right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ValidationError left, ValidationError right)
		{
			return !(left == right);
		}
	}
}
