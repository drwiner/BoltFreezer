;;;
;;; The blocks problem!
;;;
(define (problem 01)
  (:domain blocks)
  (:objects boid - steeringagent
            l0 l1 l2 l3 l4 l5 l6 l7 - location
			block1 block2 - block)
  (:init (at block1 l5)
		 (at block2 l3)
		 (at boid l0)
		 (freehands boid)
		 (occupied l5)
		 (occupied l3)
		 (occupied l0)
		 (adjacent l5 l3)
		 (adjacent l5 l6)
		 (adjacent l3 l5)
		 (adjacent l3 l4)
		 (adjacent l3 l1)
		 (adjacent l6 l5)
		 (adjacent l6 l4)
		 (adjacent l1 l3)
		 (adjacent l1 l2)
		 (adjacent l1 l0)
		 (adjacent l4 l6)
		 (adjacent l4 l3)
		 (adjacent l4 l2)
		 (adjacent l0 l1)
		 (adjacent l0 l7)
		 (adjacent l7 l2)
		 (adjacent l7 l0)
		 (adjacent l2 l4)
		 (adjacent l2 l1)
		 (adjacent l2 l7))
  (:goal (at block1 l0)))