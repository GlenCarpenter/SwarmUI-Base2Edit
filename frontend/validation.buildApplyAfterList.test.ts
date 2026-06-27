import { describe, expect, it } from "@jest/globals";
import { buildApplyAfterList } from "./validation";

describe("buildApplyAfterList", () => {
    it("returns Refiner plus filtered+sorted stage ids less than stageId", () => {
        expect(buildApplyAfterList([1, 2, 3], 3, "")).toEqual([
            "Refiner",
            "Edit Stage 1",
            "Edit Stage 2",
        ]);
    });

    it("returns only Refiner when no stageIds are less than stageId", () => {
        expect(buildApplyAfterList([1, 2, 3], 1, "")).toEqual(["Refiner"]);
    });

    it("does not duplicate currentVal when it is already in the list", () => {
        expect(buildApplyAfterList([1, 2, 3], 3, "Edit Stage 1")).toEqual([
            "Refiner",
            "Edit Stage 1",
            "Edit Stage 2",
        ]);
    });

    it("prepends stale currentVal at index 0 when not in list", () => {
        expect(buildApplyAfterList([1, 2, 3], 3, "Edit Stage 99")).toEqual([
            "Edit Stage 99",
            "Refiner",
            "Edit Stage 1",
            "Edit Stage 2",
        ]);
    });

    it("sorts unsorted stageIds ascending in output", () => {
        expect(buildApplyAfterList([3, 1, 2], 4, "")).toEqual([
            "Refiner",
            "Edit Stage 1",
            "Edit Stage 2",
            "Edit Stage 3",
        ]);
    });

    it("does not prepend empty string currentVal", () => {
        expect(buildApplyAfterList([1, 2, 3], 3, "")).toEqual([
            "Refiner",
            "Edit Stage 1",
            "Edit Stage 2",
        ]);
    });

    it("does not duplicate Refiner when currentVal is Refiner", () => {
        expect(buildApplyAfterList([1, 2, 3], 3, "Refiner")).toEqual([
            "Refiner",
            "Edit Stage 1",
            "Edit Stage 2",
        ]);
    });

    it("excludes stageId itself and any ids greater than stageId (strict less-than)", () => {
        expect(buildApplyAfterList([1, 2, 3], 2, "")).toEqual([
            "Refiner",
            "Edit Stage 1",
        ]);
    });

    it("returns only Refiner when stageIds is empty", () => {
        expect(buildApplyAfterList([], 1, "")).toEqual(["Refiner"]);
    });

    it("omits SeedVR2 by default (seedVr2Available defaults false)", () => {
        expect(buildApplyAfterList([1, 2, 3], 3, "")).toEqual([
            "Refiner",
            "Edit Stage 1",
            "Edit Stage 2",
        ]);
    });

    it("inserts SeedVR2 right after Refiner when available", () => {
        expect(buildApplyAfterList([1, 2, 3], 3, "", true)).toEqual([
            "Refiner",
            "SeedVR2",
            "Edit Stage 1",
            "Edit Stage 2",
        ]);
    });

    it("returns Refiner and SeedVR2 only when no earlier stage ids and available", () => {
        expect(buildApplyAfterList([1, 2, 3], 1, "", true)).toEqual([
            "Refiner",
            "SeedVR2",
        ]);
    });

    it("does not duplicate SeedVR2 when currentVal is SeedVR2 and available", () => {
        expect(buildApplyAfterList([1, 2, 3], 3, "SeedVR2", true)).toEqual([
            "Refiner",
            "SeedVR2",
            "Edit Stage 1",
            "Edit Stage 2",
        ]);
    });

    it("prepends stale SeedVR2 currentVal when not available", () => {
        expect(buildApplyAfterList([1, 2, 3], 3, "SeedVR2", false)).toEqual([
            "SeedVR2",
            "Refiner",
            "Edit Stage 1",
            "Edit Stage 2",
        ]);
    });
});
