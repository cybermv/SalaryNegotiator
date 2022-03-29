using System;
using System.Collections.Generic;

namespace Vele.SalaryNegotiator.Core.Dto;

public class NegotiationAdminResponse
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string EmployerName { get; set; }

    public string EmployerSecret { get; set; }

    public string EmployeeName { get; set; }

    public string EmployeeSecret { get; set; }

    public DateTime CreatedDate { get; set; }

    public List<OfferAdminResponse> Offers { get; set; }

    public override string ToString() => $"#{Id} - {Name} - '{EmployerName}' and '{EmployeeName}'";
}
