using System;

namespace ImperialHR.Api.Dtos
{
    public class CreateRequestDto
    {
        // тип заявки:
        // 0 = Vacation
        // 1 = DayOff
        // 2 = Transfer
        // 3 = Promotion
        // 4 = Dismissal
        public int Type { get; set; }

        // початок періоду
        public DateTime From { get; set; }

        // кінець періоду
        public DateTime To { get; set; }
    }
}
