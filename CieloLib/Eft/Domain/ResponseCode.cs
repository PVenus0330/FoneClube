using System;
using System.Runtime.Serialization;

namespace CieloLib.Eft.Domain
{
	public enum ResponseCode
	{
		[EnumMember(Value="00")]
		Success,
		[EnumMember(Value="01")]
		Unauthorized_Transaction,
		[EnumMember(Value="02")]
		Unauthorized_TransactionType2,
		[EnumMember(Value="03")]
		Invalid_Establishment,
		[EnumMember(Value="04")]
		Unauthorized_Transaction_Card_Blocked_By_IssuingBank,
		[EnumMember(Value="05")]
		Unauthorized_transaction_Delinquent_card,
		[EnumMember(Value="06")]
		Unauthorized_transaction_Card_canceled,
		[EnumMember(Value="07")]
		Transaction_denied_Hold_special_condition_card,
		[EnumMember(Value="08")]
		Security_code_is_invalid,
		[EnumMember(Value="11")]
		Successful,
		[EnumMember(Value="12")]
		Invalid_transaction_card_error,
		[EnumMember(Value="13")]
		Transaction_value_Invalid,
		[EnumMember(Value="14")]
		Invalid_Card,
		[EnumMember(Value="15")]
		Bank_unavailable_Or_Non_existent,
		[EnumMember(Value="19")]
		Please_redo_the_transaction_or_try_again_later,
		[EnumMember(Value="21")]
		Cancellation_snot_done_Transaction_not_found,
		[EnumMember(Value="22")]
		Invalid_installment_Number_of_invalid_parcels,
		[EnumMember(Value="23")]
		Invalid_benefit_amount,
		[EnumMember(Value="24")]
		Invalid_number_of_parcels,
		[EnumMember(Value="25")]
		Request_for_authorization_did_not_send_card_number,
		[EnumMember(Value="999")]
		InternalError
	}
}